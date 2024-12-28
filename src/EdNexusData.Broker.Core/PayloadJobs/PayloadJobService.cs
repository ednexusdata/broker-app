using EdNexusData.Broker.Common;
using System.ComponentModel;

namespace EdNexusData.Broker.Core;

public class PayloadJobService
{
    private readonly ConnectorLoader _connectorLoader;
    
    public PayloadJobService(ConnectorLoader connectorLoader)
    {
        _connectorLoader = connectorLoader;
    }

    public List<PayloadJobDisplay> GetPayloadJobs()
    {
        var connectors = _connectorLoader.Connectors;

        var payloadJobs = _connectorLoader.GetPayloadJobs()!.ToList();

        var list = new List<PayloadJobDisplay>();
        
        foreach(var payloadJob in payloadJobs)
        {
            var connector = connectors.Where(x => x.Assembly == payloadJob.Assembly).FirstOrDefault();
            
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
