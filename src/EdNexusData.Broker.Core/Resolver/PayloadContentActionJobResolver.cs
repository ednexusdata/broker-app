using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Resolvers;

public class PayloadContentActionJobResolver
{
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly FocusEducationOrganizationResolver _focusEdOrg;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public PayloadContentActionJobResolver(
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

    public PayloadContentActionJob Resolve(string payloadContentType)
    {
        var resolvedPayloadContentType = ResolveType(payloadContentType);
        _ = resolvedPayloadContentType ?? throw new ArgumentNullException($"Unable to locate payload content type / {payloadContentType}");

        var payloadContentJob = ActivatorUtilities.CreateInstance(_serviceProvider, resolvedPayloadContentType);
        
        return (PayloadContentActionJob)payloadContentJob;
    }

    public Type? ResolveType(string payloadContentType)
    {
        _ = payloadContentType ?? throw new ArgumentNullException("Missing payload content type");

        var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetExportedTypes())
                .Where(p => p.FullName == payloadContentType);

        // Locate the payload content service in connector
        return types.FirstOrDefault();
    }

    public Type? ResolveInterface(string payloadContentType, string interfaceType)
    {
        var resolvedPayloadContentType = ResolveType(payloadContentType);
        _ = resolvedPayloadContentType ?? throw new ArgumentNullException($"Unable to locate payload content type / {payloadContentType}");

        var foundInterfaceType = resolvedPayloadContentType.Assembly.GetExportedTypes()
            .Where(p => p.GetInterface(interfaceType) is not null).FirstOrDefault();

        return foundInterfaceType;
    }
}