using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Specifications;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Dynamic;
using EdNexusData.Broker.Common.Configuration;
using Microsoft.AspNetCore.DataProtection;
using System.Buffers.Text;

namespace EdNexusData.Broker.Core.Serializers;

public class ConfigurationSerializer
{
    private readonly IRepository<EducationOrganizationConnectorSettings> _repo;
    private readonly IDataProtectionProvider dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;
    
    public ConfigurationSerializer(
        IRepository<EducationOrganizationConnectorSettings> repo,
        IDataProtectionProvider dataProtectionProvider, 
        IServiceProvider serviceProvider)
    {
        _repo = repo;
        this.dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<IConfiguration> DeseralizeAsync(Type connectorConfigType, Guid focusEducationOrganization)
    {
        var iconfigModel = ActivatorUtilities.CreateInstance(_serviceProvider, connectorConfigType) as IConfiguration;
        var objTypeName = iconfigModel!.GetType().FullName;
        
        // Get existing object
        if (connectorConfigType.Assembly.GetName().Name! != null) {
            var connectorSpec = new ConnectorByNameAndEdOrgIdSpec(connectorConfigType.Assembly.GetName().Name!, focusEducationOrganization);
            var repoConnectorSettings = await _repo.FirstOrDefaultAsync(connectorSpec);
            if (repoConnectorSettings is not null && repoConnectorSettings?.Settings != null)
            {
                var configSettings = Newtonsoft.Json.Linq.JObject.Parse(repoConnectorSettings?.Settings?.RootElement.GetRawText());
                var encconfigSettingsObj = configSettings[objTypeName].ToString();

                if (!encconfigSettingsObj.Contains("{"))
                {
                    var dp = dataProtectionProvider.CreateProtector("SecureConnectionString");
                    var decryptedSerializedConfig = dp.Unprotect(encconfigSettingsObj);

                    var configSettingsObj = Newtonsoft.Json.Linq.JObject.Parse(decryptedSerializedConfig);

                    foreach(var prop in iconfigModel!.GetType().GetProperties())
                    {
                        // Check if prop in configSettings
                        var value = configSettingsObj.Value<string>(prop.Name);
                        if (value is not null)
                        {
                            prop.SetValue(iconfigModel, value);
                        }
                    }
                }
            }
        }
        // var prop = iconfigModel.GetType().GetProperty("SynergyUrl");
        // prop.SetValue(iconfigModel, "https://test.url");
        // prop = iconfigModel.GetType().GetProperty("Username");
        // prop.SetValue(iconfigModel, "TestUser");
        // prop = iconfigModel.GetType().GetProperty("Password");
        // prop.SetValue(iconfigModel, "TestPass");

        return iconfigModel!;
    }

    public async Task<IConfiguration> SerializeAndSaveAsync(EdNexusData.Broker.Common.Configuration.IConfiguration obj, Guid focusEducationOrganization)
    {
        var repoConnectorSettings = new EducationOrganizationConnectorSettings();

        var objType = obj.GetType();
        var objTypeName = objType.FullName;
        var objAssemblyName = objType.Assembly.GetName().Name!;

        // Get existing record, if there is one
        var connectorSpec = new ConnectorByNameAndEdOrgIdSpec(objAssemblyName, focusEducationOrganization);
        var prevRepoConnectorSettings = await _repo.FirstOrDefaultAsync(connectorSpec);
        if (prevRepoConnectorSettings is not null)
        {
            repoConnectorSettings = prevRepoConnectorSettings;
        }

        // Merge to existing object, if exists
        // if (repoConnectorSettings.Settings != null)
        // {
        //     var deseralizedSettings = await DeseralizeAsync(objType, focusEducationOrganization);
        //     foreach(var prop in obj.GetType().GetProperties())
        //     {
        //         prop.SetValue(repoConnectorSettings.Settings, prop.GetValue(obj));
        //     }
        //     obj = deseralizedSettings;
        // }

        // Encrypt object
        var dp = dataProtectionProvider.CreateProtector("SecureConnectionString");
        var seralizedConfig = JsonSerializer.SerializeToDocument<dynamic>(obj);
        var encryptedSerializedConfig = dp.Protect(seralizedConfig.ToJsonString()!);

        // Serialize settings object
        //dynamic objWrapper = new ExpandoObject();
        //((IDictionary<String, String>)objWrapper)[objTypeName!] = encryptedSerializedConfig;

        var objWrapper = new Dictionary<string, string>();
        objWrapper[objTypeName!] = encryptedSerializedConfig;

        var seralizedIConfigModel = JsonSerializer.SerializeToDocument<dynamic>(objWrapper);
        repoConnectorSettings.Settings = seralizedIConfigModel;

        if (objAssemblyName != null && repoConnectorSettings.Id != Guid.Empty) {
            await _repo.UpdateAsync(repoConnectorSettings);
        }
        else
        {
            repoConnectorSettings.EducationOrganizationId = focusEducationOrganization;
            repoConnectorSettings.Connector = objAssemblyName!;
            await _repo.AddAsync(repoConnectorSettings);
        }

        return obj;
    }
}