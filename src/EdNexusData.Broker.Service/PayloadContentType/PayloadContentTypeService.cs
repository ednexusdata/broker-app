using EdNexusData.Broker.Connector;
using EdNexusData.Broker.Connector.PayloadContentTypes;
using System.ComponentModel;

namespace EdNexusData.Broker.Service;

public class PayloadContentTypeService
{
    private readonly ConnectorLoader _connectorLoader;
    
    public PayloadContentTypeService(ConnectorLoader connectorLoader)
    {
        _connectorLoader = connectorLoader;
    }

    public List<PayloadContentTypeDisplay> GetPayloadContentTypes()
    {
        var connectors = _connectorLoader.Connectors;

        var payloadContentTypes = _connectorLoader.GetContentTypes()!.ToList();

        var list = new List<PayloadContentTypeDisplay>();
        
        foreach(var payloadContentType in payloadContentTypes)
        {
            var connector = connectors.Where(x => x.Assembly == payloadContentType.Assembly).FirstOrDefault();
            
            var display = new PayloadContentTypeDisplay
            {
                DisplayName = ((DisplayNameAttribute)connector!
                    .GetCustomAttributes(false)
                    .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName + " / " 
                  + ((DisplayNameAttribute)payloadContentType
                    .GetCustomAttributes(false)
                    .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName ?? payloadContentType.Name,
                Name = payloadContentType.Name,
                FullName = payloadContentType.FullName!,
                AllowMultiple = (bool?)payloadContentType.GetProperty("AllowMultiple")?.GetValue(null) ?? false,
                AllowConfiguration = (bool?)payloadContentType.GetProperty("AllowConfiguration")?.GetValue(null) ?? false
            };

            list.Add(display);
        }
        
        return list;
    }
}
