using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Services;

namespace EdNexusData.Broker.Core.Updaters;

public class ConnectorUpdater
{
    private readonly HttpClient httpClient;
    private readonly ConnectorService connectorService;
    private readonly ConnectorLoader connectorLoader;

    public ConnectorUpdater(
        IHttpClientFactory httpClientFactory,
        ConnectorService connectorService,
        ConnectorLoader connectorLoader
    )
    {
        httpClient = httpClientFactory.CreateClient();
        this.connectorService = connectorService;
        this.connectorLoader = connectorLoader;
    }

    public async Task Synchronize()
    {
        var connectorsToOperate = await Analyze();

        
    }

    public async Task<List<(StoreConnector, ConnectorReference)>> Analyze()
    {
        // Get store list
        var storeList = await connectorService.GetStoreConnectors();
        // Get connector references
        var connectorReferences = await connectorService.GetConnectorReferences();
        // Get connectors installed
        var connectorsLoaded = connectorLoader.Connectors;

        var connectorToOperate = new List<(StoreConnector, ConnectorReference)>();

        // Detect if connector reference installed
        foreach(var connectorReference in connectorReferences)
        {
            var connectorLoaded = connectorsLoaded.Where(x => x.FullName == connectorReference.Reference).First();
            var version = connectorLoaded.Assembly.GetName().Version;
            
            var storeConnector = storeList?.Where(x => x.ReferenceName == connectorReference.Reference).FirstOrDefault();
            var latestRelease = storeConnector?.LatestRelease()?.ToSystemVersion();

            if (connectorLoaded is not null)
            {
                // Connector installed and check version
                if (version?.CompareTo(latestRelease) < 0)
                {
                    connectorToOperate.Add((storeConnector, connectorReference)!);
                }
            }
            else
            {
                connectorToOperate.Add((storeConnector, connectorReference)!);
            }
        }

        return connectorToOperate;
    }


}