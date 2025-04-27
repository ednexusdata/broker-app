using EdNexusData.Broker.Common;
using EdNexusData.Broker.Core.Specifications;
using System.ComponentModel;

namespace EdNexusData.Broker.Core;

public class PayloadJobService
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly IRepository<EducationOrganizationConnectorSettings> edOrgConnectorSettingsRepo;

    public PayloadJobService(
        ConnectorLoader connectorLoader,
        IRepository<EducationOrganizationConnectorSettings> edOrgConnectorSettingsRepo
    )
    {
        _connectorLoader = connectorLoader;
        this.edOrgConnectorSettingsRepo = edOrgConnectorSettingsRepo;
    }

    public async Task<List<PayloadJobDisplay>> GetPayloadJobs(Guid educationOrganizationId)
    {
        var connectors = _connectorLoader.Connectors;

        var enabledConnectors = await edOrgConnectorSettingsRepo.ListAsync(new EnabledConnectorsByEdOrgSpec(educationOrganizationId));

        var payloadJobs = _connectorLoader.GetPayloadJobs()!.ToList();

        var list = new List<PayloadJobDisplay>();
        
        foreach(var payloadJob in payloadJobs)
        {
            var connector = connectors.Where(x => 
                   x.Assembly == payloadJob.Assembly 
                && enabledConnectors.Exists(y => y.Connector == x.Assembly.GetName().Name)
                ).FirstOrDefault();

            if (connector is null) continue;
            
            var display = new PayloadJobDisplay
            {
                DisplayName = ((DisplayNameAttribute)connector!
                    .GetCustomAttributes(false)
                    .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName + " / " 
                  + ((DisplayNameAttribute)payloadJob
                    .GetCustomAttributes(false)
                    .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName ?? payloadJob.Name,
                Name = payloadJob.Name,
                FullName = payloadJob.FullName!,
                AllowMultiple = (bool?)payloadJob.GetField("AllowMultiple")?.GetValue(null) ?? false,
                AllowConfiguration = (bool?)payloadJob.GetField("AllowConfiguration")?.GetValue(null) ?? false
            };

            list.Add(display);
        }
        
        return list;
    }
}
