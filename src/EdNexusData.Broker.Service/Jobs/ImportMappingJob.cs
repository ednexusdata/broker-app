using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Service.Worker;
using EdNexusData.Broker.Service.Resolvers;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using EdNexusData.Broker.Domain.Worker;
using System.ComponentModel;

namespace EdNexusData.Broker.Service.Jobs;

[Description("Import Mapping")]
public class ImportMappingJob : IJob
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConnectorResolver _connectorResolver;
    private readonly PayloadResolver _payloadResolver;
    private readonly JobStatusService<ImportMappingJob> _jobStatusService;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Domain.PayloadContent> _payloadContentRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly FocusEducationOrganizationResolver _focusEducationOrganizationResolver;

    public ImportMappingJob(
            ConnectorLoader connectorLoader,
            ConnectorResolver connectorResolver,
            PayloadResolver payloadResolver,
            JobStatusService<ImportMappingJob> jobStatusService,
            IRepository<Request> requestRepository,
            IRepository<Domain.PayloadContent> payloadContentRepository,
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
        _serviceProvider = serviceProvider;
        _mappingRepository = mappingRepository;
        _focusEducationOrganizationResolver = focusEducationOrganizationResolver;
    }
    
    public async Task ProcessAsync(Job jobInstance)
    {
        Guard.Against.Null(jobInstance.ReferenceGuid, "referenceGuid", $"Missing reference mapping id in job {jobInstance.ReferenceGuid}");
        
        var mapping = await _mappingRepository.FirstOrDefaultAsync(new MappingWithPayloadContent(jobInstance.ReferenceGuid.Value));

        Guard.Against.Null(mapping, "mapping", $"Unable to find mapping Id {jobInstance.ReferenceGuid}");
        Guard.Against.Null(mapping.PayloadContentAction, "mapping", $"Unable to load mapping with payload content action {jobInstance.ReferenceGuid}");
        Guard.Against.Null(mapping.PayloadContentAction.PayloadContent, "mapping", $"Unable to load payload content with payload content action {jobInstance.ReferenceGuid}");
        
        if (mapping.PayloadContentAction.Process == false)
        {
            throw new ArgumentException($"Mapping set not to process. Stopping. {jobInstance.ReferenceGuid}");
        }

        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(mapping.PayloadContentAction.PayloadContent.RequestId));
        
        Guard.Against.Null(request, "request", $"Unable to find request id {jobInstance.ReferenceGuid}");
        
        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, Domain.PayloadContentActionStatus.Importing, "Begin import mapping for: {0}", request.Payload);
        
        // Set the ed org
        _focusEducationOrganizationResolver.EducationOrganizationId = request.EducationOrganization!.ParentOrganizationId!.Value;

        // Get incoming payload settings
        var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync(request.Payload, request.EducationOrganization!.ParentOrganizationId!.Value);

        // Resolve the SIS connector
        Guard.Against.Null(payloadSettings.StudentInformationSystem, null, "No SIS incoming connector set.");
        var sisConnectorType = _connectorResolver.Resolve(payloadSettings.StudentInformationSystem);
        Guard.Against.Null(sisConnectorType, null, "Unable to load connector.");

        var importers = new Dictionary<Type, dynamic>();

        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, Domain.PayloadContentActionStatus.Importing, "Begin processing map with type: {0}.", mapping.MappingType);

        // Deseralize object to the type
        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, Domain.PayloadContentActionStatus.Importing, "Will deseralize object of type: {0}.", mapping.MappingType);
        var mappingType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetExportedTypes())
                    .Where(p => p.FullName == mapping.MappingType).FirstOrDefault();
        Guard.Against.Null(mappingType, null, $"Unable to find concrete type {mapping.MappingType}");

        Type mappingCollectionType = typeof(List<>).MakeGenericType([mappingType]);

        dynamic mappingCollection = JsonConvert.DeserializeObject(mapping.JsonDestinationMapping.ToJsonString()!, mappingCollectionType)!;

        // Find appropriate importer
        var importerType = _connectorLoader.Importers.Where(x => x.Key == mappingType).FirstOrDefault().Value;

        Guard.Against.Null(importerType, null, $"Unable to find importer for {mappingType}");

        var methodInfo = importerType.GetMethod("Prepare");

        Guard.Against.Null(methodInfo, null, $"Unable to find Prepare on mapped type {importerType.FullName}");

        dynamic importer;

        // See if not created
        if (!importers.TryGetValue(importerType, out importer!))
        {
            importer = ActivatorUtilities.CreateInstance(_serviceProvider, importerType);
            importers.Add(importerType, importer);
            await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, Domain.PayloadContentActionStatus.Importing, "Created importer of type {0}.", importerType.FullName);
        }
        
        methodInfo!.Invoke(importer, new object[] { mappingType, mappingCollection, request.RequestManifest?.Student!, request.EducationOrganization, request.ResponseManifest! });

        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, Domain.PayloadContentActionStatus.Importing, "Called prepare on {0}.", importerType.FullName);

        // Call finish method on each importer
        methodInfo = importerType.GetMethod("ImportAsync");
        var result = await methodInfo!.Invoke(importer, new object[] { });

        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, Domain.PayloadContentActionStatus.Importing, "Called import on {0} and it returned {1}.", importerType.FullName, result);

        await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, Domain.PayloadContentActionStatus.Imported, "Finished importing mapping.");
    }
}