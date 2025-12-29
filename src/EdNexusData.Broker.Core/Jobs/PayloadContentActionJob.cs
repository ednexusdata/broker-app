using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Core.Resolvers;
using EdNexusData.Broker.Core.Specifications;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using EdNexusData.Broker.Common.Students;
using System.ComponentModel;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Common.Mappings;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Process Payload Content Actions")]
public class PayloadContentActionJob : IJob
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConnectorResolver _connectorResolver;
    private readonly PayloadResolver _payloadResolver;
    private readonly PayloadContentActionJobResolver payloadContentActionJobResolver;
    private readonly JobStatusService<PayloadContentActionJob> jobStatusService;
    private readonly IRepository<PayloadContentAction> _payloadContentActionRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly FocusEducationOrganizationResolver _focusEducationOrganizationResolver;

    public PayloadContentActionJob(
            ConnectorLoader connectorLoader,
            ConnectorResolver connectorResolver,
            PayloadResolver payloadResolver,
            PayloadContentActionJobResolver payloadContentActionJobResolver,
            JobStatusService<PayloadContentActionJob> jobStatusService,
            IRepository<PayloadContentAction> payloadContentActionRepository,
            IServiceProvider serviceProvider,
            IRepository<Mapping> mappingRepository,
            FocusEducationOrganizationResolver focusEducationOrganizationResolver)
    {
        _connectorLoader = connectorLoader;
        _connectorResolver = connectorResolver;
        _payloadResolver = payloadResolver;
        this.payloadContentActionJobResolver = payloadContentActionJobResolver;
        this.jobStatusService = jobStatusService;
        _payloadContentActionRepository = payloadContentActionRepository;
        _serviceProvider = serviceProvider;
        _mappingRepository = mappingRepository;
        _focusEducationOrganizationResolver = focusEducationOrganizationResolver;
    }
    
    public async Task ProcessAsync(Job jobInstance)
    {
        // Step 1: Get the payload content action
         _ = jobInstance.ReferenceGuid ?? throw new ArgumentNullException($"Unable to find request Id {jobInstance.ReferenceGuid}");
        var payloadContentAction = await _payloadContentActionRepository.FirstOrDefaultAsync(new PayloadContentActionWithPayloadContent(jobInstance.ReferenceGuid.Value));
       
        _ = payloadContentAction ?? throw new ArgumentNullException($"Couldn't find payload content action id {jobInstance.ReferenceGuid}");
        _ = payloadContentAction.PayloadContent ?? throw new ArgumentNullException($"Unable to load payload content with payload content action {jobInstance.ReferenceGuid}");
        _ = payloadContentAction.PayloadContent.Request ?? throw new ArgumentNullException($"Unable to load request with payload content action {jobInstance.ReferenceGuid}");
        _ = payloadContentAction.PayloadContent.Request.EducationOrganization ?? throw new ArgumentNullException($"Unable to load request with payload content action {jobInstance.ReferenceGuid}");
        _ = payloadContentAction.PayloadContent.Request.EducationOrganization.ParentOrganizationId ?? throw new ArgumentNullException($"Unable to load request with payload content action {jobInstance.ReferenceGuid}");
        _ = payloadContentAction.ActiveMapping ?? throw new ArgumentNullException($"Unable to load active mapping with payload content action {jobInstance.ReferenceGuid}");
        _ = payloadContentAction.PayloadContentActionType ?? throw new ArgumentNullException($"Unable to load active mapping with payload content action type {jobInstance.ReferenceGuid}");
        
        await jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, PayloadContentActionStatus.Importing, "Fetched payload content action.");
        
        // Step 2: Setup the input parameters
        
        // Get incoming payload settings
        _focusEducationOrganizationResolver.EducationOrganizationId = payloadContentAction.PayloadContent.Request.EducationOrganization.ParentOrganizationId.Value;
        var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync(payloadContentAction.PayloadContent.Request.Payload, payloadContentAction.PayloadContent.Request.EducationOrganization!.ParentOrganizationId!.Value);

        // Resolve the SIS connector
        _ = payloadSettings.StudentInformationSystem ?? throw new ArgumentNullException($"No SIS incoming connector set");
        var sisConnectorType = _connectorResolver.ResolveConnector(payloadSettings.StudentInformationSystem);
        _ = sisConnectorType ?? throw new ArgumentNullException($"Unable to load connector");

        // Resolve Student
        _ = payloadContentAction.PayloadContent.Request.Student ?? throw new ArgumentNullException($"Student does not exist.");
        var connectorStudentType = payloadContentActionJobResolver.ResolveInterface(payloadContentAction.PayloadContentActionType, nameof(IStudent));

        dynamic? connectorStudent = null;

        if (connectorStudentType is not null) 
        {
            // See if student type exists in connectors
            if (payloadContentAction.PayloadContent.Request.Student.Connectors!.Count > 0)
            {
                var foundStudentType = payloadContentAction.PayloadContent.Request.Student.Connectors.Where(x => x.Key == connectorStudentType.FullName).FirstOrDefault();
                connectorStudent = JsonConvert.DeserializeObject(foundStudentType.Value.ToString()!, connectorStudentType)!;
            }
        }

        // Create mapping object
        var mapping = payloadContentAction.ActiveMapping;
        _ = mapping.MappingType ?? throw new ArgumentNullException($"Mapping type does not exist.");
        var mappingType = payloadContentActionJobResolver.ResolveType(mapping.MappingType);
        _ = mappingType ?? throw new ArgumentNullException($"Unable to load mapping type {mapping.MappingType}");

        var listMappingType = typeof(List<>).MakeGenericType(mappingType);
        dynamic? mappingObject = JsonConvert.DeserializeObject(mapping.JsonDestinationMapping.ToJsonString()!, listMappingType)!;
        dynamic? mappingObjectsToImport = ActivatorUtilities.CreateInstance(_serviceProvider, listMappingType);

        // keep objects that are not to be imported
        foreach(dynamic map in mappingObject)
        {
            if (map.BrokerMappingRecordAction == MappingRecordAction.Import)
            {
                mappingObjectsToImport.Add(map);
            }
        }

        if (mappingObjectsToImport.Count == 0)
        {
            await jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, PayloadContentActionStatus.Error, "No mapping objects to import.");
            return;
        }

        // Step 3: Create the payload content action job object
        await jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, PayloadContentActionStatus.Importing, "Begin processing map with type: {0}.", mapping.MappingType);

        // Resolve payload content action type
        await jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, PayloadContentActionStatus.Importing, "Will deseralize object of type: {0}.", mapping.MappingType);
        // Create payload content action
        var payloadContentActionJobObject = payloadContentActionJobResolver.Resolve(payloadContentAction.PayloadContentActionType);
        _ = payloadContentActionJobObject ?? throw new ArgumentNullException($"Unable to load payload content action job object {payloadContentAction.PayloadContentActionType}");

        // Step 4: Execute the payload content action job object
        object? result = null;
        
        // Attach job status info
            

        try
        {
            if (payloadContentActionJobObject is DelayedPayloadContentActionJob)
            {
                DelayedPayloadContentActionJob delayedJobToExecute = (DelayedPayloadContentActionJob)payloadContentActionJobObject!;
                delayedJobToExecute.JobStatusService = new JobStatusServiceProxy<PayloadContentActionJob>(jobStatusService, jobInstance, payloadContentAction.PayloadContent.Request);

                var startResult = await delayedJobToExecute.StartAsync(
                    mappingObjectsToImport,
                    payloadContentAction.ToCommon(),
                    connectorStudent!,
                    payloadContentAction.PayloadContent.Request.Student!.Student!.ToCommon(),
                    payloadContentAction.PayloadContent.Request.EducationOrganization.ToCommon(),
                    delayedJobToExecute.JobStatusService
                );

                if (startResult == DelayedJobStatus.Finish)
                {
                    result = await delayedJobToExecute.FinishAsync(delayedJobToExecute.JobStatusService);
                }
                else
                {
                    var continueLooping = true;
                    DelayedJobStatus? continueResult = null;
                    while (continueLooping)
                    {
                        await Task.Delay(5000);
                        continueResult = await delayedJobToExecute.ContinueAsync(delayedJobToExecute.JobStatusService);
                        if (continueResult != DelayedJobStatus.Continue)
                            continueLooping = false;
                    }

                    if (continueResult is not null && continueResult == DelayedJobStatus.Finish)
                    {
                        result = await delayedJobToExecute.FinishAsync(delayedJobToExecute.JobStatusService);
                    }
                }
            }
            else
            {
                // Call execute on it
                result = await payloadContentActionJobObject.ExecuteAsync(
                    mappingObjectsToImport,
                    payloadContentAction.ToCommon(),
                    connectorStudent!,
                    payloadContentAction.PayloadContent.Request.Student!.Student!.ToCommon(),
                    payloadContentAction.PayloadContent.Request.EducationOrganization.ToCommon(),
                    new JobStatusServiceProxy<PayloadContentActionJob>(jobStatusService, jobInstance, payloadContentAction.PayloadContent.Request)
                );
            }

            await jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, PayloadContentActionStatus.Imported, result?.ToString());
        }
        catch (Exception e)
        {
            await jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, PayloadContentActionStatus.Error, "Errored with: {0}.", e.Message);
            throw;
        }
    }
}