using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Service.Worker;
using EdNexusData.Broker.Service.Resolvers;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using EdNexusData.Broker.Domain.Worker;
using EdNexusData.Broker.Common.Students;
using System.ComponentModel;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Common.Mappings;

namespace EdNexusData.Broker.Service.Jobs;

[Description("Process Payload Content Actions")]
public class PayloadContentActionJob : IJob
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConnectorResolver _connectorResolver;
    private readonly PayloadResolver _payloadResolver;
    private readonly JobStatusService<PayloadContentActionJob> _jobStatusService;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<PayloadContentAction> _payloadContentActionRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<Domain.Mapping> _mappingRepository;
    private readonly FocusEducationOrganizationResolver _focusEducationOrganizationResolver;

    public PayloadContentActionJob(
            ConnectorLoader connectorLoader,
            ConnectorResolver connectorResolver,
            PayloadResolver payloadResolver,
            JobStatusService<PayloadContentActionJob> jobStatusService,
            IRepository<Request> requestRepository,
            IRepository<PayloadContentAction> payloadContentActionRepository,
            IServiceProvider serviceProvider,
            IRepository<Domain.Mapping> mappingRepository,
            FocusEducationOrganizationResolver focusEducationOrganizationResolver)
    {
        _connectorLoader = connectorLoader;
        _connectorResolver = connectorResolver;
        _payloadResolver = payloadResolver;
        _jobStatusService = jobStatusService;
        _requestRepository = requestRepository;
        _payloadContentActionRepository = payloadContentActionRepository;
        _serviceProvider = serviceProvider;
        _mappingRepository = mappingRepository;
        _focusEducationOrganizationResolver = focusEducationOrganizationResolver;
    }
    
    public async Task ProcessAsync(Job jobInstance)
    {
        Guard.Against.Null(jobInstance.ReferenceGuid, "referenceGuid", $"Missing reference payload content id in job {jobInstance.ReferenceGuid}");

        var payloadContentAction = await _payloadContentActionRepository.FirstOrDefaultAsync(new PayloadContentActionWithPayloadContent(jobInstance.ReferenceGuid.Value));

        Guard.Against.Null(payloadContentAction, "payloadContentAction", "Couldn't find payload content action id");
        Guard.Against.Null(payloadContentAction.PayloadContent, "payloadContentAction.PayloadContent", "Unable to load payload content with payload content action {jobInstance.ReferenceGuid}");
        Guard.Against.Null(payloadContentAction.PayloadContent.Request, "payloadContentAction.PayloadContent.Request", "Unable to load request with payload content action {jobInstance.ReferenceGuid}");
        Guard.Against.Null(payloadContentAction.PayloadContent.Request.EducationOrganization, "payloadContentAction.PayloadContent.Request.EducationOrganization", "Unable to load request with payload content action {jobInstance.ReferenceGuid}");
        Guard.Against.Null(payloadContentAction.PayloadContent.Request.EducationOrganization.ParentOrganizationId, "payloadContentAction.PayloadContent.Request.EducationOrganization.ParentOrganizationId", "Unable to load request with payload content action {jobInstance.ReferenceGuid}");
        
        //var mapping = await _mappingRepository.FirstOrDefaultAsync(new ActiveMappingForPayloadContentAction(payloadContentAction.ActiveMappingId!.Value));

        Guard.Against.Null(payloadContentAction.ActiveMapping, "mapping", $"Unable to find mapping Id {jobInstance.ReferenceGuid}");

        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, Domain.PayloadContentActionStatus.Importing, "Fetched payload content action.");
        await _jobStatusService.UpdateRequestStatus(jobInstance, payloadContentAction.PayloadContent.Request, RequestStatus.InProgress, "Importing.");
        
        var mapping = payloadContentAction.ActiveMapping;
 
        // Set the ed org
        _focusEducationOrganizationResolver.EducationOrganizationId = payloadContentAction.PayloadContent.Request.EducationOrganization.ParentOrganizationId.Value;

        // Get incoming payload settings
        var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync(payloadContentAction.PayloadContent.Request.Payload, payloadContentAction.PayloadContent.Request.EducationOrganization!.ParentOrganizationId!.Value);

        // Resolve the SIS connector
        Guard.Against.Null(payloadSettings.StudentInformationSystem, null, "No SIS incoming connector set.");
        var sisConnectorType = _connectorResolver.Resolve(payloadSettings.StudentInformationSystem);
        Guard.Against.Null(sisConnectorType, null, "Unable to load connector.");

        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, Domain.PayloadContentActionStatus.Importing, "Begin processing map with type: {0}.", mapping.MappingType);

        // Resolve payload content action type
        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, Domain.PayloadContentActionStatus.Importing, "Will deseralize object of type: {0}.", mapping.MappingType);
        var payloadContentActionType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetExportedTypes())
                    .Where(p => p.FullName == payloadContentAction.PayloadContentActionType).FirstOrDefault();
        Guard.Against.Null(payloadContentActionType, null, $"Unable to find concrete type {payloadContentAction.PayloadContentActionType}");

        // Create payload content action
        dynamic payloadContentActionObject = ActivatorUtilities.CreateInstance(_serviceProvider, payloadContentActionType);

        // Call execute on it
        var methodInfo = payloadContentActionType.GetMethod("ExecuteAsync");

        Guard.Against.Null(methodInfo, null, $"Unable to find method ExecuteAsync on payload content action: {payloadContentAction.PayloadContentActionType}");

        Guard.Against.Null(payloadContentAction.PayloadContent.Request.Student, "payloadContentAction.PayloadContent.Request.Student", "Student does not exist.");

        // Get IStudent type in connector
        var connectorStudentType = payloadContentActionType.Assembly.GetExportedTypes()
            .Where(p => p.GetInterface(nameof(IStudent)) is not null).FirstOrDefault();

        dynamic? connectorStudent = null;

        if (connectorStudentType is not null) 
        {
            // See if student type exists in connectors
            if (payloadContentAction.PayloadContent.Request.Student.Connectors!.Count > 0)
            {
                var foundStudentType = payloadContentAction.PayloadContent.Request.Student.Connectors.Where(x => x.Key == connectorStudentType.FullName).FirstOrDefault();
                
                connectorStudent = JsonConvert.DeserializeObject(foundStudentType.Value.ToString(), connectorStudentType)!;
            }
        }

        // Create mapping object
        var mappingType = payloadContentActionType.Assembly.GetExportedTypes()
            .Where(p => p.FullName == mapping.MappingType).FirstOrDefault();

        Guard.Against.Null(mappingType, "mappingType", $"Unable to find mapping type {mappingType}");

        var listMappingType = typeof(List<>).MakeGenericType(mappingType);

        dynamic? mappingObject = JsonConvert.DeserializeObject(mapping.JsonDestinationMapping.ToJsonString(), listMappingType)!;

        dynamic? mappingObjectsToImport = ActivatorUtilities.CreateInstance(_serviceProvider, listMappingType);

        // keep objects that are not to imported
        foreach(dynamic map in mappingObject)
        {
            if (map.BrokerMappingRecordAction == MappingRecordAction.Import)
            {
                mappingObjectsToImport.Add(map);
            }
        }

        if (mappingObjectsToImport.Count > 0)
        {
            // object mapping, 
            // PayloadContentAction payloadContentAction, 
            // IStudent student, 
            // Domain.Student brokerStudent, 
            // EducationOrganization educationOrganization
            var result = await methodInfo!.Invoke(payloadContentActionObject, new object[] { 
                mappingObjectsToImport, 
                payloadContentAction,
                connectorStudent!, 
                payloadContentAction.PayloadContent.Request.Student!.Student!, 
                payloadContentAction.PayloadContent.Request.EducationOrganization
            });

            await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, payloadContentAction, Domain.PayloadContentActionStatus.Imported, result.ToString());
            await _jobStatusService.UpdateRequestStatus(jobInstance, payloadContentAction.PayloadContent.Request, RequestStatus.InProgress, "Imported.");
        }

    }
}