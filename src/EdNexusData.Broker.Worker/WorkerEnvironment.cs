namespace EdNexusData.Broker.Worker;

public class WorkerEnvironment : Core.Environment
{
    public WorkerEnvironment(IConfiguration configuration)
    {
        EnvironmentName = configuration.GetValue<string>("Environment") ?? throw new ArgumentNullException("Missing environment");

        if (IsNonProductionToLocalEnvironment())
        {
            var urls = configuration.GetValue<string>("EnvironmentUrls") ?? throw new ArgumentNullException("Missing environment urls");
            var splitUrls = urls.Split(",");
            foreach(var url in splitUrls)
            {
                AddAddress(url);
            }
        }

        var brokerBaseUrl = configuration.GetValue<string>("BrokerBaseUrl");
        _ = brokerBaseUrl ?? throw new ArgumentNullException("Missing broker base url");

        BrokerBaseUrl = new Uri(brokerBaseUrl);
    }
}