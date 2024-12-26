using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Core.Payloads;

namespace EdNexusData.Broker.Service.Resolvers;

public class PayloadResolver : IPayloadResolver
{
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public PayloadResolver(
        IRepository<EducationOrganizationPayloadSettings> edOrgPayloadSettings, 
        DistrictEducationOrganizationResolver districtEdOrg,
        IServiceProvider serviceProvider
    )
    {
        _edOrgPayloadSettings = edOrgPayloadSettings;
        _districtEdOrg = districtEdOrg;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<Core.PayloadSettings.IncomingPayloadSettings> FetchIncomingPayloadSettingsAsync<T>(Guid educationOrganizationId) where T : Payload
    {
        return await FetchIncomingPayloadSettingsAsync(typeof(T), educationOrganizationId);
    }

    public async Task<Core.PayloadSettings.IncomingPayloadSettings> FetchIncomingPayloadSettingsAsync(string payloadType, Guid educationOrganizationId)
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetExportedTypes())
                .Where(p => p.FullName == payloadType);

        var foundPayloadType = types.FirstOrDefault();

        Guard.Against.Null(foundPayloadType, "Unable to resolve payloadType");
        
        return await FetchIncomingPayloadSettingsAsync(foundPayloadType, educationOrganizationId);
    }

    public async Task<Core.PayloadSettings.IncomingPayloadSettings> FetchIncomingPayloadSettingsAsync(Type t, Guid educationOrganizationId)
    {
        Guard.Against.Null(t);
        
        var connectorSpec = new PayloadSettingsByNameAndEdOrgIdSpec(t.FullName!, await _districtEdOrg.Resolve(educationOrganizationId));
        var repoConnectorSettings = await _edOrgPayloadSettings.FirstOrDefaultAsync(connectorSpec);
        
        Guard.Against.Null(repoConnectorSettings);
        Guard.Against.Null(repoConnectorSettings.IncomingPayloadSettings);

        return repoConnectorSettings!.IncomingPayloadSettings.ToContract();
    }

    public async Task<Core.PayloadSettings.OutgoingPayloadSettings> FetchOutgoingPayloadSettingsAsync<T>(Guid educationOrganizationId) where T : Payload
    {
        return await FetchOutgoingPayloadSettingsAsync(typeof(T), educationOrganizationId);
    }

    public async Task<Core.PayloadSettings.OutgoingPayloadSettings> FetchOutgoingPayloadSettingsAsync(string payloadType, Guid educationOrganizationId)
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetExportedTypes())
                .Where(p => p.FullName == payloadType);

        var foundPayloadType = types.FirstOrDefault();

        Guard.Against.Null(foundPayloadType, "Unable to resolve payloadType");
        
        return await FetchOutgoingPayloadSettingsAsync(foundPayloadType, educationOrganizationId);
    }

    public async Task<Core.PayloadSettings.OutgoingPayloadSettings> FetchOutgoingPayloadSettingsAsync(Type t, Guid educationOrganizationId)
    {
        Guard.Against.Null(t);

        var connectorSpec = new PayloadSettingsByNameAndEdOrgIdSpec(t.FullName!, await _districtEdOrg.Resolve(educationOrganizationId));
        var repoConnectorSettings = await _edOrgPayloadSettings.FirstOrDefaultAsync(connectorSpec);

        Guard.Against.Null(repoConnectorSettings);
        Guard.Against.Null(repoConnectorSettings.OutgoingPayloadSettings);

        return repoConnectorSettings!.OutgoingPayloadSettings.ToContract();
    }
}