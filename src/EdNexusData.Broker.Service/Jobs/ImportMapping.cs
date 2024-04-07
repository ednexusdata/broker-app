using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using System.Text.Json;
using EdNexusData.Broker.Service.Worker;
using EdNexusData.Broker.Service.Resolvers;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Domain.Specifications;
using System.Text.Json.Nodes;
using System.Reflection;
using EdNexusData.Broker.Connector;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EdNexusData.Broker.Service.Jobs;

public class ImportMapping
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConnectorResolver _connectorResolver;
    private readonly PayloadResolver _payloadResolver;
    private readonly JobStatusService<SendRequest> _jobStatusService;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly FocusEducationOrganizationResolver _focusEducationOrganizationResolver;

    public ImportMapping(
            ConnectorLoader connectorLoader,
            ConnectorResolver connectorResolver,
            PayloadResolver payloadResolver,
            JobStatusService<SendRequest> jobStatusService,
            IRepository<PayloadContent> payloadContentRepository,
            IServiceProvider serviceProvider,
            IRepository<Mapping> mappingRepository,
            FocusEducationOrganizationResolver focusEducationOrganizationResolver)
    {
        _connectorLoader = connectorLoader;
        _connectorResolver = connectorResolver;
        _payloadResolver = payloadResolver;
        _jobStatusService = jobStatusService;
        _payloadContentRepository = payloadContentRepository;
        _serviceProvider = serviceProvider;
        _mappingRepository = mappingRepository;
        _focusEducationOrganizationResolver = focusEducationOrganizationResolver;
    }
    
    public async Task Process(Request request)
    {
        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Importing, "Begin import mapping for: {0}", request.Payload);

        // Set the ed org
        _focusEducationOrganizationResolver.EducationOrganizationId = request.EducationOrganization!.ParentOrganizationId!.Value;

        // Get incoming payload settings
        var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync(request.Payload, request.EducationOrganization!.ParentOrganizationId!.Value);

        // Resolve the SIS connector
        Guard.Against.Null(payloadSettings.StudentInformationSystem, null, "No SIS incoming connector set.");
        var sisConnectorType = _connectorResolver.Resolve(payloadSettings.StudentInformationSystem);
        Guard.Against.Null(sisConnectorType, null, "Unable to load connector.");

        // Get mappings
        var mappings = await _mappingRepository.ListAsync(new MappingByRequestId(request.Id));

        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Importing, "Found {0} mappings for request.", mappings.Count);

        var importers = new Dictionary<Type, dynamic>();

        // For each file run, extract contents and collapse to distinct types
        foreach(var mapping in mappings)
        {
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Importing, "Begin processing map with type: {0}.", mapping.MappingType);

            // Deseralize object to the type
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Importing, "Will deseralize object of type: {0}.", mapping.MappingType);
            var mappingType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetExportedTypes())
                        .Where(p => p.FullName == mapping.MappingType).FirstOrDefault();
            Guard.Against.Null(mappingType, null, $"Unable to find concrete type {mapping.MappingType}");

            Type mappingCollectionType = typeof(List<>).MakeGenericType([mappingType]);

            dynamic mappingCollection = JsonConvert.DeserializeObject(mapping.DestinationMapping.ToJsonString()!, mappingCollectionType)!;

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
                await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Importing, "Created importer of type {0}.", importerType.FullName);
            }
            
            methodInfo!.Invoke(importer, new object[] { mappingType, mappingCollection, request.RequestManifest?.Student!, request.EducationOrganization, request.ResponseManifest! });

            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Importing, "Called prepare on {0}.", importerType.FullName);
        }

        // Call finish method on each importer
        foreach(var (importerType, importer) in importers)
        {
            var methodInfo = importerType.GetMethod("ImportAsync");
            var result = await methodInfo!.Invoke(importer, new object[] { });
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Importing, "Called import on {0} and it returned {1}.", importerType.FullName, result);
        }

        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Imported, "Finished importing mapping.");
    }
}