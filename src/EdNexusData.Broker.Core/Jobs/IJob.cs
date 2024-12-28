using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core;

public interface IJob
{
    public Task ProcessAsync(Job jobRecord);
}