using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Resolvers;

public class PayloadJobResolver
{
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly FocusEducationOrganizationResolver _focusEdOrg;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public PayloadJobResolver(
        IRepository<EducationOrganizationPayloadSettings> edOrgPayloadSettings, 
        FocusEducationOrganizationResolver focusEdOrg, 
        DistrictEducationOrganizationResolver districtEdOrg,
        IServiceProvider serviceProvider
    )
    {
        _edOrgPayloadSettings = edOrgPayloadSettings;
        _focusEdOrg = focusEdOrg;
        _districtEdOrg = districtEdOrg;
        _serviceProvider = serviceProvider;
    }

    public PayloadJob Resolve(string payloadContentType)
    {
        Guard.Against.Null(payloadContentType);

        var resolvedPayloadContentType = ConnectorLoader.Instance?.ResolveType(payloadContentType);

        Guard.Against.Null(resolvedPayloadContentType, "", "Could not get payload content type");

        var payloadContentJob = ActivatorUtilities.CreateInstance(_serviceProvider, resolvedPayloadContentType);
        
        return (PayloadJob)payloadContentJob;
    }
}