using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Dynamic;
using EdNexusData.Broker.Connector.Payload;

namespace EdNexusData.Broker.Service.Serializers;

public class IncomingPayloadSerializer
{
    private readonly IRepository<EducationOrganizationPayloadSettings> _repo;
    private readonly IServiceProvider _serviceProvider;

    public IncomingPayloadSerializer(IRepository<EducationOrganizationPayloadSettings> repo, IServiceProvider serviceProvider)
    {
        _repo = repo;
        _serviceProvider = serviceProvider;
    }

    public async Task<IPayload> DeseralizeAsync(Type connectorConfigType, Guid focusEducationOrganization)
    {
        var iPayloadModel = ActivatorUtilities.CreateInstance(_serviceProvider, connectorConfigType) as IPayload;
        var objTypeName = iPayloadModel!.GetType().FullName;

        // Get existing object
        if (connectorConfigType.Assembly.GetName().Name != null)
        {
            var connectorSpec = new PayloadByNameAndEdOrgIdSpec(connectorConfigType?.FullName!, focusEducationOrganization);
            var repoConnectorSettings = await _repo.FirstOrDefaultAsync(connectorSpec);
            if (repoConnectorSettings is not null)
            {
                var configSettings = repoConnectorSettings.IncomingPayloadSettings!.PayloadContents;

                //var configSettingsObj = configSettings[objTypeName];

                foreach (var prop in iPayloadModel!.GetType().GetProperties())
                {
                    // Check if prop in configSettings
                    var value = configSettings; // TODO .Where(i => i.PayloadContentType == prop.Name)
                    if (value is not null)
                    {
                        prop.SetValue(iPayloadModel, value);
                    }
                }
            }
        }
        return iPayloadModel!;
    }

    public async Task<IPayload> SerializeAndSaveAsync(IPayload obj, Guid focusEducationOrganization)
    {
        var repoConnectorSettings = new EducationOrganizationPayloadSettings();

        var objType = obj.GetType();
        var objTypeName = objType.FullName;
        var objAssemblyName = objType.Assembly.GetName().Name!;

        // Get existing record, if there is one
        var connectorSpec = new PayloadByNameAndEdOrgIdSpec(objAssemblyName, focusEducationOrganization);
        var prevRepoConnectorSettings = await _repo.FirstOrDefaultAsync(connectorSpec);
        if (prevRepoConnectorSettings is not null)
        {
            repoConnectorSettings = prevRepoConnectorSettings;
        }

        dynamic objWrapper = new ExpandoObject();
        ((IDictionary<string, object>)objWrapper)[objTypeName!] = obj;

        var seralizedIConfigModel = JsonSerializer.SerializeToDocument<dynamic>(objWrapper);
        repoConnectorSettings.IncomingPayloadSettings!.PayloadContents = seralizedIConfigModel;

        if (objAssemblyName != null && repoConnectorSettings.Id != Guid.Empty)
        {
            await _repo.UpdateAsync(repoConnectorSettings);
        }
        else
        {
            repoConnectorSettings.EducationOrganizationId = focusEducationOrganization;
            repoConnectorSettings.Payload = objAssemblyName!;
            await _repo.AddAsync(repoConnectorSettings);
        }

        return obj;
    }
}