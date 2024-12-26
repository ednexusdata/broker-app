using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Service.Resolvers;

public class ConnectorResolver
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public ConnectorResolver(
        ConnectorLoader connectorLoader,
        IRepository<EducationOrganizationPayloadSettings> edOrgPayloadSettings, 
        DistrictEducationOrganizationResolver districtEdOrg,
        IServiceProvider serviceProvider
    )
    {
        _connectorLoader = connectorLoader;
        _edOrgPayloadSettings = edOrgPayloadSettings;
        _districtEdOrg = districtEdOrg;
        _serviceProvider = serviceProvider;
    }
    
    public Type? Resolve(string connectorTypeName)
    {
        return  _connectorLoader.Connectors.Where(x => x.FullName == connectorTypeName).FirstOrDefault();
    }
}