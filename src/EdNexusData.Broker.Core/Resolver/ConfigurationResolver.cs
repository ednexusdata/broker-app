using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Specifications;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Common.Configuration;
using Microsoft.AspNetCore.DataProtection;

namespace EdNexusData.Broker.Core.Resolvers;

public class ConfigurationResolver : IConfigurationResolver
{
    private readonly IRepository<EducationOrganizationConnectorSettings> _edOrgConnectorSettings;
    private readonly IDataProtectionProvider dataProtectionProvider;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly FocusEducationOrganizationResolver _focusEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public Guid? CurrentRecordEducationOrganizationId { get; set; }

    public ConfigurationResolver(
        IRepository<EducationOrganizationConnectorSettings> edOrgConnectorSettings, 
        IDataProtectionProvider dataProtectionProvider,
        DistrictEducationOrganizationResolver districtEdOrg,
        FocusEducationOrganizationResolver focusEdOrg, 
        IServiceProvider serviceProvider
    )
    {
        _edOrgConnectorSettings = edOrgConnectorSettings;
        this.dataProtectionProvider = dataProtectionProvider;
        _districtEdOrg = districtEdOrg;
        _focusEdOrg = focusEdOrg;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<T> FetchConnectorSettingsAsync<T>()
    {
        if (CurrentRecordEducationOrganizationId != null)
        {
            return await FetchConnectorSettingsAsync<T>(CurrentRecordEducationOrganizationId.Value);
        }
        return await FetchConnectorSettingsAsync<T>(_districtEdOrg.Resolve(await _focusEdOrg.Resolve()).Id);
    }

    public async Task<T> FetchConnectorSettingsAsync<T>(Guid educationOrganizationId)
    {
        Guard.Against.Null(typeof(T).Assembly.GetName().Name);

        // Get existing object
        var connectorSpec = new ConnectorByNameAndEdOrgIdSpec(typeof(T).Assembly.GetName().Name!, educationOrganizationId);
        var repoConnectorSettings = await _edOrgConnectorSettings.FirstOrDefaultAsync(connectorSpec);

        Guard.Against.Null(repoConnectorSettings);

        return DeseralizeConnectorSettingsAsync<T>(repoConnectorSettings);
    }

    public async Task<T> FetchConnectorSettingsAsync<T>(string districtNumber)
    {
        Guard.Against.Null(typeof(T).Assembly.GetName().Name);

        // Get existing object
        var connectorSpec = new ConnectorByNameAndDistrictNumberSpec(typeof(T).Assembly.GetName().Name!, districtNumber);
        var repoConnectorSettings = await _edOrgConnectorSettings.FirstOrDefaultAsync(connectorSpec);

        Guard.Against.Null(repoConnectorSettings);

        return DeseralizeConnectorSettingsAsync<T>(repoConnectorSettings);
    }

    private T DeseralizeConnectorSettingsAsync<T>(EducationOrganizationConnectorSettings repoConnectorSettings)
    {
        var iconfigModel = (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T));
        var objTypeName = iconfigModel.GetType().FullName;
        
        Guard.Against.Null(typeof(T).Assembly.GetName().Name);

        var configSettings = Newtonsoft.Json.Linq.JObject.Parse(repoConnectorSettings?.Settings?.RootElement.GetRawText());

        var encconfigSettingsObj = configSettings[objTypeName].ToString();

        var dp = dataProtectionProvider.CreateProtector("SecureConnectionString");
        var decryptedSerializedConfig = dp.Unprotect(encconfigSettingsObj);

        var configSettingsObj = Newtonsoft.Json.Linq.JObject.Parse(decryptedSerializedConfig);

        foreach(var prop in iconfigModel!.GetType().GetProperties())
        {
            // Check if prop in configSettings
            var value = configSettingsObj.Value<string>(prop.Name);

            Guard.Against.Null(value);
            
            prop.SetValue(iconfigModel, value);
        }

        return iconfigModel;
    }
}