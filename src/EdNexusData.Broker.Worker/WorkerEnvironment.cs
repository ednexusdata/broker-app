namespace EdNexusData.Broker.Worker;

public class WorkerEnvironment : Core.Environment
{
    public WorkerEnvironment(IConfiguration configuration)
    {
        EnvironmentName = configuration.GetValue<string>("Environment") ?? throw new ArgumentNullException("Missing environment");

        if (IsNonProductionEnvironment())
        {
            var urls = configuration.GetValue<string>("EnvironmentUrls") ?? throw new ArgumentNullException("Missing environment urls");
            var splitUrls = urls.Split(",");
            foreach(var url in splitUrls)
            {
                AddAddress(url);
            }
        }
    }
}