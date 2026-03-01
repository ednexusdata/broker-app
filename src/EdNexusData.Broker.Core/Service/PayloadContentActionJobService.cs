using System.ComponentModel;
using System.Text.Json;
using DnsClient.Internal;
using EdNexusData.Broker.Common.Connector;
using EdNexusData.Broker.Core.Serializers;
using EdNexusData.Broker.Core.Specifications;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Core.Services;

public class PayloadContentActionJobService
{
    private readonly ILogger<PayloadContentActionJobService> logger;
    private readonly ConnectorLoader connectorLoader;
    private readonly IRepository<EducationOrganizationConnectorSettings> edOrgConnectorSettingsRepo;

    public PayloadContentActionJobService(
        ILogger<PayloadContentActionJobService> logger,
        ConnectorLoader connectorLoader,
        IRepository<EducationOrganizationConnectorSettings> edOrgConnectorSettingsRepo)
    {
        this.logger = logger;
        this.connectorLoader = connectorLoader;
        this.edOrgConnectorSettingsRepo = edOrgConnectorSettingsRepo;
    }

    public async Task<List<Type>> RespondsTo(PayloadContent payloadContent, EducationOrganization educationOrganization)
    {
        var resolvedPayloadContentActions = new List<Type>();
        
        var contentPayloadActions = connectorLoader.GetPayloadContentActions();

        // Determine payload content actions that can respond to this payload content
        foreach (var contentPayloadAction in contentPayloadActions!)
        {
            var connectorNameForPayloadAction = contentPayloadAction.Assembly.GetName().Name;

            // If connector is enabled
            var enabledConnectors = await edOrgConnectorSettingsRepo.ListAsync(new EnabledConnectorsByEdOrgSpec(educationOrganization.Id));
            if (!enabledConnectors.Any(x => x.Connector == connectorNameForPayloadAction))
            {
                logger.LogWarning($"No active connectors found for {educationOrganization} while finding payload content action jobs that respond.");
                continue;
            }
            
            // and if the connector has a transformer that responds to this payload content
            if (payloadContent.ContentType == "application/json" && payloadContent.JsonContent is not null)
            {
                var payloadContentObject = DataPayloadContentSerializer.Deseralize(payloadContent.JsonContent.ToJsonString()!);
                var payloadContentSchema = payloadContentObject.Schema;
            
                var transformerType = connectorLoader.Transformers.Where(x => 
                    x.Key == $"{contentPayloadAction.Assembly.GetName().Name}::{payloadContentSchema?.Schema}::{payloadContentSchema?.SchemaVersion}").ToDictionary();

                if (transformerType.Count > 0)
                {
                    resolvedPayloadContentActions.Add(contentPayloadAction);
                }
            }
        }
        
        return resolvedPayloadContentActions;
    }

    public async Task<List<Type>?> RespondsToNew(PayloadContent payloadContent, EducationOrganization educationOrganization)
    {
        var resolvedPayloadContentActions = new List<Type>();
        
        // Get active connectors for Education Organization
        var enabledConnectors = await edOrgConnectorSettingsRepo.ListAsync(new EnabledConnectorsByEdOrgSpec(educationOrganization.Id));
        if (enabledConnectors is null || enabledConnectors.Count == 0)
        {
            logger.LogWarning($"No active connectors found for {educationOrganization} while finding payload content action jobs that respond.");
            return null;
        }

        if (payloadContent.ContentType == "application/json" && payloadContent.JsonContent is not null)
        {
            var payloadContentObject = DataPayloadContentSerializer.Deseralize(payloadContent.JsonContent.ToJsonString()!);
            var payloadContentSchema = payloadContentObject.Schema;

            foreach (var enabledConnector in enabledConnectors!)
            {
                // Find transformer that responds
                var transformerTypes = connectorLoader.Transformers.Where(x => 
                    x.Key == $"{enabledConnector.Connector}::{payloadContentSchema?.Schema}::{payloadContentSchema?.SchemaVersion}").Select(x => x.Value).ToList();

                if (transformerTypes.Count > 0)
                {
                    foreach(var transfomer in transformerTypes)
                    {
                        // Find payload content action job that processes transformer
                        var payloadContentActionJobTypes = connectorLoader.PayloadContentActionByTransformer.Where(
                            x => x.Key == $"{enabledConnector.Connector}::{payloadContentSchema?.Schema}::{payloadContentSchema?.SchemaVersion}");

                        resolvedPayloadContentActions.AddRange(payloadContentActionJobTypes.Select(x => x.Value).ToList());
                    }
                }
            }
        }
        
        return resolvedPayloadContentActions;
    }
}