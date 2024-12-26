using EdNexusData.Broker.Domain;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Service.Resolvers;

public class AuthenticationProviderResolver
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public AuthenticationProviderResolver(
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
    
    public IAuthenticationProvider? Resolve(string connectorTypeName)
    {
        Type? authProviderType = _connectorLoader.GetAuthenticator(connectorTypeName);  

        Guard.Against.Null(authProviderType, "authProviderType", $"Unable to resole {connectorTypeName}");

        return (IAuthenticationProvider)ActivatorUtilities.CreateInstance(_serviceProvider, authProviderType);  
    }
}