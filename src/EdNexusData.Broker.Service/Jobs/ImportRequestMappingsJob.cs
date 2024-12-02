using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Service.Worker;
using EdNexusData.Broker.Service.Resolvers;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Domain.Internal.Specifications;
using EdNexusData.Broker.Connector;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using EdNexusData.Broker.Domain.Worker;
using System.ComponentModel;

namespace EdNexusData.Broker.Service.Jobs;

[Description("Import Request Mapping")]
public class ImportRequestMappingsJob : IJob
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConnectorResolver _connectorResolver;
    private readonly PayloadResolver _payloadResolver;
    private readonly JobStatusService<ImportRequestMappingsJob> _jobStatusService;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Domain.PayloadContent> _payloadContentRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly FocusEducationOrganizationResolver _focusEducationOrganizationResolver;

    public ImportRequestMappingsJob(
            ConnectorLoader connectorLoader,
            ConnectorResolver connectorResolver,
            PayloadResolver payloadResolver,
            JobStatusService<ImportRequestMappingsJob> jobStatusService,
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
        Guard.Against.Null(jobInstance.ReferenceGuid, "referenceGuid", $"Unable to find request Id {jobInstance.ReferenceGuid}");
        
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(jobInstance.ReferenceGuid.Value));
        
        Guard.Against.Null(request, "request", $"Unable to find request id {jobInstance.ReferenceGuid}");
        
        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.InProgress, "Begin import mapping for: {0}", request.Payload);
        
        // Set the ed org
        _focusEducationOrganizationResolver.EducationOrganizationId = request.EducationOrganization!.ParentOrganizationId!.Value;

        // Get incoming payload settings
        var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync(request.Payload, request.EducationOrganization!.ParentOrganizationId!.Value);

        // Resolve the SIS connector
        Guard.Against.Null(payloadSettings.StudentInformationSystem, null, "No SIS incoming connector set.");
        var sisConnectorType = _connectorResolver.Resolve(payloadSettings.StudentInformationSystem);
        Guard.Against.Null(sisConnectorType, null, "Unable to load connector.");

        // Get mappings
        var mappings = await _mappingRepository.ListAsync(new MappingByActionId(request.Id));

        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.InProgress, "Found {0} mappings for job.", mappings.Count);

        var importers = new Dictionary<Type, dynamic>();

        // For each file run, extract contents and collapse to distinct types
        foreach(var mapping in mappings.Where(x => x.PayloadContentAction?.Process == true).ToList())
        {
            Guard.Against.Null(mapping.PayloadContentAction, null, $"PayloadContentAction not loaded on mapping {mapping.Id}");
            await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, PayloadContentActionStatus.Importing, "Begin processing map with type: {0}.", mapping.MappingType);

            // Deseralize object to the type
            await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, PayloadContentActionStatus.Importing, "Will deseralize object of type: {0}.", mapping.MappingType);
            var mappingType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetExportedTypes())
                        .Where(p => p.FullName == mapping.MappingType).FirstOrDefault();
            Guard.Against.Null(mappingType, null, $"Unable to find concrete type {mapping.MappingType}");

            Type mappingCollectionType = typeof(List<>).MakeGenericType([mappingType]);

            dynamic mappingCollection = JsonConvert.DeserializeObject(mapping.JsonDestinationMapping.ToJsonString()!, mappingCollectionType)!;

            // Find appropriate importer
            var importerType = _connectorLoader.Importers.Where(x => x.Key == mappingType).FirstOrDefault().Value;

            if (importerType is null) { continue; }

            var methodInfo = importerType.GetMethod("Prepare");

            if (methodInfo is null) { continue; }

            dynamic importer;

            // See if not created
            if (!importers.TryGetValue(importerType, out importer!))
            {
                importer = ActivatorUtilities.CreateInstance(_serviceProvider, importerType);
                importers.Add(importerType, importer);
                await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, PayloadContentActionStatus.Importing, "Created importer of type {0}.", importerType.FullName);
            }
            
            methodInfo!.Invoke(importer, new object[] { mappingType, mappingCollection, request.RequestManifest?.Student!, request.EducationOrganization, request.ResponseManifest! });

            await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction, PayloadContentActionStatus.Importing, "Called prepare on {0}.", importerType.FullName);
        }

        // Call finish method on each importer
        foreach(var (importerType, importer) in importers)
        {
            var methodInfo = importerType.GetMethod("ImportAsync");
            var result = await methodInfo!.Invoke(importer, new object[] { });
            await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.InProgress, "Called import on {0} and it returned {1}.", importerType.FullName, result);
        }

        foreach(var mapping in mappings.Where(x => x.PayloadContentAction?.Process == true).ToList())
        {
            await _jobStatusService.UpdatePayloadContentActionStatus(jobInstance, mapping.PayloadContentAction!, PayloadContentActionStatus.Imported, "Finished importing {0}.", mapping.MappingType);
        }

        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.InProgress, "Finished importing all mappings for request.");
    }
}