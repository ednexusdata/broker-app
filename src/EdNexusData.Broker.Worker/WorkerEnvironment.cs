namespace EdNexusData.Broker.Worker;

public class WorkerEnvironment : Core.Environment
{
    public WorkerEnvironment(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
    {
        ApplicationName = Core.ApplicationName.EdNexusDataBrokerWorker;
        EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
    }
}