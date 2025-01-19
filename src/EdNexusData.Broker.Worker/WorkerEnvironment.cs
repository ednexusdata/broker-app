namespace EdNexusData.Broker.Worker;

public class WorkerEnvironment : Core.Environment
{
    public WorkerEnvironment(IConfiguration configuration)
    {
        EnvironmentName = configuration.GetValue<string>("Environment") ?? throw new ArgumentNullException("Missing environment");
    }
}