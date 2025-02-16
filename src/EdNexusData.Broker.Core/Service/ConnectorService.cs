using System.Net.Http.Json;
using EdNexusData.Broker.Core.Models;

namespace EdNexusData.Broker.Core.Services;

public class ConnectorService
{
    private readonly IRepository<ConnectorReference> connectorReferenceRepository;
    private readonly HttpClient httpClient;
    
    public ConnectorService(
        IRepository<ConnectorReference> connectorReferenceRepository,
        IHttpClientFactory httpClientFactory
    )
    {
        this.connectorReferenceRepository = connectorReferenceRepository;
        httpClient = httpClientFactory.CreateClient();
    }

    public async Task<List<StoreConnector>?> GetStoreConnectors()
    {
        var response = await httpClient.GetAsync("https://connectors.broker.ednexusdata.org");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<StoreConnector>>();

        result = result!.OrderBy(x => x.Company).OrderBy(x => x.Name).ToList();
        
        return result;
    }

    public async Task<StoreConnector?> GetStoreConnector(string referenceName)
    {
        var connectors = await GetStoreConnectors();
        _ = connectors ?? throw new NullReferenceException("No store connectors returned");
        
        var connector = connectors.Where(x => x.ReferenceName == referenceName).FirstOrDefault();
        return connector;
    }

    public async Task<List<ConnectorReference>> GetConnectorReferences()
    {
        var connectors = await connectorReferenceRepository.ListAsync();
        return connectors;
    }

    public async Task<ConnectorReference?> GetConnectorReference(Guid id)
    {
        return await connectorReferenceRepository.GetByIdAsync(id);
    }

    public async Task<ConnectorReference> AddConnectorReference(StoreConnector connector)
    {
        var connectorAdded = await connectorReferenceRepository.AddAsync(connector.ToConnectorReference());
        return connectorAdded;
    }

    public async Task RemoveConnectorReference(ConnectorReference connector)
    {
        await connectorReferenceRepository.DeleteAsync(connector);
    }
}