using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using System.Text.Json;
using EdNexusData.Broker.Service.Worker;
using EdNexusData.Broker.Service.Resolvers;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.Connector;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Domain.Worker;

namespace EdNexusData.Broker.Service.Jobs;

public class PrepareMappingJob : IJob
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConnectorResolver _connectorResolver;
    private readonly PayloadResolver _payloadResolver;
    private readonly JobStatusService<PrepareMappingJob> _jobStatusService;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Domain.PayloadContent> _payloadContentRepository;
    private readonly IRepository<PayloadContentAction> _actionRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<Mapping> _mappingRepository;

    public PrepareMappingJob(
            ConnectorLoader connectorLoader,
            ConnectorResolver connectorResolver,
            PayloadResolver payloadResolver,
            JobStatusService<PrepareMappingJob> jobStatusService,
            IRepository<Request> requestRepository,
            IRepository<Domain.PayloadContent> payloadContentRepository,
            IRepository<PayloadContentAction> actionRepository,
            IServiceProvider serviceProvider,
            IRepository<Mapping> mappingRepository)
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

        // Resolve the SIS connector
        Guard.Against.Null(payloadSettings.StudentInformationSystem, null, "No SIS incoming connector set.");
        var sisConnectorType = _connectorResolver.Resolve(payloadSettings.StudentInformationSystem);
        Guard.Against.Null(sisConnectorType, null, "Unable to load connector.");

        // Extract contents and collapse to distinct types
        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Begin processing file {0} with schema {1}.", payloadContent.FileName, payloadContent.ContentType);

        Guard.Against.Null(payloadContent.JsonContent, null, "No Json content in retrieved content payload.");

        // Get Schema
        var payloadContentSchemaJson = payloadContent.JsonContent?.RootElement.GetProperty("Schema");
        Guard.Against.Null(payloadContentSchemaJson, null, "Missing schema element.");
        var payloadContentSchema = JsonSerializer.Deserialize<PayloadContentSchema>(payloadContentSchemaJson.ToString()!);
        Guard.Against.Null(payloadContentSchema?.ObjectType, null, "Schema missing");

        // Deseralize object to the type
        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Will deseralize object of type: {0}.", payloadContentSchema.ObjectType);
        var payloadContentSchemaType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetExportedTypes())
                    .Where(p => p.FullName == payloadContentSchema.ObjectType).FirstOrDefault();
        Guard.Against.Null(payloadContentSchemaType, null, $"Unable to find concrete type {payloadContentSchema?.ObjectType}");
        
        dynamic payloadContentObject = Convert.ChangeType(JsonSerializer.Deserialize(payloadContent.JsonContent!, payloadContentSchemaType), payloadContentSchemaType)!;

        // Find appropriate transformer
        var transformerType = _connectorLoader.Transformers.Where(x => x.Key == $"{sisConnectorType.Assembly.GetName().Name}::{payloadContentSchema?.Schema}::{payloadContentSchema?.SchemaVersion}").FirstOrDefault().Value;
        
        if (transformerType is null)
        { 
            Guard.Against.Null(transformerType, null, $"Unable to resolve transformer type: {sisConnectorType.Assembly.GetName().Name}::{payloadContentSchema?.Schema}::{payloadContentSchema?.SchemaVersion}");
        }
        var methodInfo = transformerType.GetMethod("Map");

        if (methodInfo is null)
        { 
            Guard.Against.Null(transformerType, null, $"Unable to find method map on transformerType: {transformerType.FullName}");
        }

        var transformMethodInfo = transformerType.GetMethod("Transform");

        var transformerContentType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetExportedTypes())
            .Where(p => p.FullName == payloadContentSchema?.ContentObjectType).FirstOrDefault();

        var records = new List<dynamic>();
        var transformedRecords = new List<dynamic>();

        var contentRecords = JsonSerializer.Deserialize<List<dynamic>>(payloadContentObject.Content);

        Type? recordType = null;

        foreach(var record in contentRecords)
        {

            var correctRecordType = Convert.ChangeType(JsonSerializer.Deserialize(record, transformerContentType, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }), transformerContentType);

            // Run through connector's transformer
            dynamic transformer = ActivatorUtilities.CreateInstance(_serviceProvider, transformerType);
            var result = methodInfo!.Invoke(transformer, new object[] { 
                correctRecordType, 
                payloadContent.Request.RequestManifest?.Student!, 
                payloadContent.Request.EducationOrganization, 
                payloadContent.Request.ResponseManifest!
            });
            
            recordType = result.GetType();

            var transformResult = result;

            if (transformMethodInfo is not null)
            {
                transformResult = transformMethodInfo!.Invoke(transformer, new object[] { 
                    correctRecordType, 
                    payloadContent.Request.RequestManifest?.Student!, 
                    payloadContent.Request.EducationOrganization, 
                    payloadContent.Request.ResponseManifest!
                });
                transformResult.BrokerId = result.BrokerId;
            }
            
            // Save each
            records.Add(result);
            transformedRecords.Add(transformResult);
        }

        List<dynamic>? outRecords = null;
        List<dynamic>? outTransformedRecords = null;

        outRecords = (List<dynamic>)transformerType.GetMethod("Sort")?.Invoke(null, [records])!;
        outTransformedRecords = (List<dynamic>)transformerType.GetMethod("Sort")?.Invoke(null, [transformedRecords])!;
        
        var recordsSerialized = JsonSerializer.SerializeToDocument((outRecords is null) ? records : outRecords);
        var transformedRecordsSerialized = JsonSerializer.SerializeToDocument((outTransformedRecords is null) ? transformedRecords : outTransformedRecords);

        await _mappingRepository.AddAsync(new Mapping()
        {
            PayloadContentActionId = action.Id,
            OriginalSchema = payloadContentSchema,
            MappingType = recordType?.FullName,
            StudentAttributes = null,
            JsonSourceMapping = recordsSerialized,
            JsonDestinationMapping = transformedRecordsSerialized
        });

        await _jobStatusService.UpdateRequestStatus(jobInstance, payloadContent.Request, RequestStatus.Prepared, "Prepared");

        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Complete, "Finished preparing mapping.");
    }
}