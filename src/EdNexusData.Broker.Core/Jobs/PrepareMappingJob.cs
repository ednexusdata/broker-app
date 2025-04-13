using System.Text.Json;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Core.Resolvers;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Core.Specifications;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using EdNexusData.Broker.Common.Jobs;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using EdNexusData.Broker.Core.Serializers;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Prepare Mapping")]
public class PrepareMappingJob : IJob
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConnectorResolver _connectorResolver;
    private readonly PayloadResolver _payloadResolver;
    private readonly JobStatusService<PrepareMappingJob> _jobStatusService;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<PayloadContentAction> _actionRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly FocusEducationOrganizationResolver focusEducationOrganizationResolver;

    public PrepareMappingJob(
            ConnectorLoader connectorLoader,
            ConnectorResolver connectorResolver,
            PayloadResolver payloadResolver,
            JobStatusService<PrepareMappingJob> jobStatusService,
            IRepository<Request> requestRepository,
            IRepository<Core.PayloadContent> payloadContentRepository,
            IRepository<PayloadContentAction> actionRepository,
            IServiceProvider serviceProvider,
            IRepository<Mapping> mappingRepository,
            FocusEducationOrganizationResolver focusEducationOrganizationResolver)
    {
        _connectorLoader = connectorLoader;
        _connectorResolver = connectorResolver;
        _payloadResolver = payloadResolver;
        _jobStatusService = jobStatusService;
        _requestRepository = requestRepository;
        _payloadContentRepository = payloadContentRepository;
        _actionRepository = actionRepository;
        _serviceProvider = serviceProvider;
        _mappingRepository = mappingRepository;
        this.focusEducationOrganizationResolver = focusEducationOrganizationResolver;
    }
    
    public async Task ProcessAsync(Job jobInstance)
    {
        Guard.Against.Null(jobInstance.ReferenceGuid, "referenceGuid", $"Missing action id {jobInstance.ReferenceGuid}");

        var action = await _actionRepository.GetByIdAsync(jobInstance.ReferenceGuid.Value);

        Guard.Against.Null(action, "action", "Unable to find action.");
        Guard.Against.Null(action.PayloadContentId, "action.PayloadContentId", "Action missing payload content ID.");

        var payloadContent = await _payloadContentRepository.FirstOrDefaultAsync(new PayloadContentsWithRequest(action.PayloadContentId.Value));
        
        Guard.Against.Null(payloadContent, "payloadContent", $"Unable to find payload content id {jobInstance.ReferenceGuid}");
        Guard.Against.Null(payloadContent.Request, "payloadContent.Request", $"Payload content missing request {jobInstance.ReferenceGuid}");
        
        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Begin preparing mapping for: {0}", payloadContent.FileName);

        // Get incoming payload settings
        var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync(payloadContent.Request.RequestManifest!.RequestType, payloadContent.Request.EducationOrganization!.ParentOrganizationId!.Value);

        // Set the ed org
        focusEducationOrganizationResolver.EducationOrganizationId = payloadContent.Request.EducationOrganization!.ParentOrganizationId!.Value;

        // Resolve the SIS connector
        // Guard.Against.Null(payloadSettings.StudentInformationSystem, null, "No SIS incoming connector set.");
        // var sisConnectorType = _connectorResolver.Resolve(payloadSettings.StudentInformationSystem);
        // Guard.Against.Null(sisConnectorType, null, "Unable to load connector.");

        // Resolve the selected payload content action
        _ = action.PayloadContentActionType ?? throw new NullReferenceException($"Missing payload content action type.");
        var payloadContentActionType = _connectorResolver.ResolvePayloadContentAction(action.PayloadContentActionType);
        _ = payloadContentActionType ?? throw new NullReferenceException($"Unable to resolve payload content action type: {action.PayloadContentActionType}");

        // Extract contents and collapse to distinct types
        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Begin processing file {0} with schema {1}.", payloadContent.FileName, payloadContent.ContentType);

        Guard.Against.Null(payloadContent.JsonContent, null, "No Json content in retrieved content payload.");

        var payloadContentObject = DataPayloadContentSerializer.Deseralize(payloadContent.JsonContent.ToJsonString()!);
        var payloadContentSchema = payloadContentObject.Schema;

        // // Get Schema
        // var payloadContentSchemaJson = payloadContent.JsonContent?.RootElement.GetProperty("Schema");
        // Guard.Against.Null(payloadContentSchemaJson, null, "Missing schema element.");
        // var payloadContentSchema = System.Text.Json.JsonSerializer.Deserialize<PayloadContentSchema>(payloadContentSchemaJson.ToString()!);
        // Guard.Against.Null(payloadContentSchema?.ObjectType, null, "Schema missing");

        // // Deseralize object to the type
        // await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Will deseralize object of type: {0}.", payloadContentSchema.ObjectType);
        // var payloadContentSchemaType = AppDomain.CurrentDomain.GetAssemblies()
        //             .SelectMany(s => s.GetExportedTypes())
        //             .Where(p => p.FullName == payloadContentSchema.ObjectType).FirstOrDefault();
        // Guard.Against.Null(payloadContentSchemaType, null, $"Unable to find concrete type {payloadContentSchema?.ObjectType}");
        
        // // Extract the "Attributes" node
        // JsonElement root = payloadContent.JsonContent!.RootElement;
        // JsonElement additionalNode = root.GetProperty("AdditionalContents");

        // // Reconstruct the JSON without the "Attributes" node
        // JsonNode rootNode = JsonNode.Parse(payloadContent.JsonContent!.ToJsonString()!)!;
        // JsonNode additionalRootNode = rootNode["AdditionalContents"]!;
        // rootNode["AdditionalContents"] = null;

        // var test = rootNode.ToJsonString();

        // var jsondeserialize = JsonConvert.DeserializeObject(test, payloadContentSchemaType); // JsonSerializer.Deserialize(payloadContent.JsonContent!, payloadContentSchemaType);
        // dynamic payloadContentObject = Convert.ChangeType(jsondeserialize, payloadContentSchemaType)!;

        // Find appropriate transformer
        var transformerType = _connectorLoader.Transformers.Where(x => x.Key == $"{payloadContentActionType.Assembly.GetName().Name}::{payloadContentSchema?.Schema}::{payloadContentSchema?.SchemaVersion}").FirstOrDefault().Value;
        if (transformerType is null)
        { 
            await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, action, PayloadContentActionStatus.Error, "Error");
            throw new NullReferenceException($"Unable to resolve transformer type: {payloadContentActionType.Assembly.GetName().Name}::{payloadContentSchema?.Schema}::{payloadContentSchema?.SchemaVersion}");
        }

        var methodInfo = transformerType.GetMethod("Map");
        _ = methodInfo ?? throw new MissingMethodException($"Unable to find method map on transformerType: {transformerType.FullName}");

        var transformMethodInfo = transformerType.GetMethod("Transform");

        var transformerContentType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetExportedTypes())
            .Where(p => p.FullName == payloadContentSchema?.ContentObjectType).FirstOrDefault();

        var records = new List<dynamic>();
        var transformedRecords = new List<dynamic>();

        var contentRecords = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(payloadContentObject!.Content!.ToString()!);

        Type? recordType = null;

        // Create transformer object
        dynamic transformer = ActivatorUtilities.CreateInstance(_serviceProvider, transformerType);

        foreach(var record in contentRecords!)
        {

            var correctRecordType = Convert.ChangeType(System.Text.Json.JsonSerializer.Deserialize(record, transformerContentType, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }), transformerContentType);

            // Call transformer's map method
            var result = methodInfo!.Invoke(transformer, new object[] { 
                correctRecordType, 
                payloadContent.Request.RequestManifest?.Student?.ToCommon()!, 
                payloadContent.Request.EducationOrganization?.ToCommon()!, 
                payloadContent.Request.ResponseManifest?.ToCommon()!,
                payloadContentObject.AdditionalContents!
            });

            var awaitedResult = await result;
            
            recordType = awaitedResult.GetType();

            var transformResult = awaitedResult;
            dynamic awaitedTransformedResult = transformResult;

            if (transformMethodInfo is not null)
            {
                transformResult = transformMethodInfo!.Invoke(transformer, new object[] { 
                    correctRecordType, 
                    payloadContent.Request.RequestManifest?.Student?.ToCommon()!, 
                    payloadContent.Request.EducationOrganization?.ToCommon()!, 
                    payloadContent.Request.ResponseManifest?.ToCommon()!,
                    payloadContentObject.AdditionalContents!
                });

                awaitedTransformedResult = await transformResult;

                awaitedTransformedResult.BrokerId = awaitedResult.BrokerId;
            }
            
            // Save each
            records.Add(awaitedResult);
            transformedRecords.Add(awaitedTransformedResult);
        }

        List<dynamic>? outRecords = null;
        List<dynamic>? outTransformedRecords = null;

        outRecords = (List<dynamic>)transformerType.GetMethod("Sort")?.Invoke(null, [records])!;
        outTransformedRecords = (List<dynamic>)transformerType.GetMethod("Sort")?.Invoke(null, [transformedRecords])!;
        
        var recordsSerialized = System.Text.Json.JsonSerializer.SerializeToDocument((outRecords is null) ? records : outRecords);
        var transformedRecordsSerialized = System.Text.Json.JsonSerializer.SerializeToDocument((outTransformedRecords is null) ? transformedRecords : outTransformedRecords);

        var newMapping = new Mapping()
        {
            PayloadContentActionId = action.Id,
            OriginalSchema = PayloadContentSchema.ToCore(payloadContentSchema!),
            MappingType = recordType?.FullName,
            StudentAttributes = null,
            JsonSourceMapping = recordsSerialized,
            JsonInitialMapping = transformedRecordsSerialized,
            JsonDestinationMapping = transformedRecordsSerialized
        };

        await _mappingRepository.AddAsync(newMapping);

        action.ActiveMappingId = newMapping.Id;
        await _actionRepository.UpdateAsync(action);

        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, action, Core.PayloadContentActionStatus.Prepared, "Prepared");

        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Complete, "Finished preparing mapping.");
    }
}