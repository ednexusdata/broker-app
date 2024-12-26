using EdNexusData.Broker.Domain.Worker;

namespace EdNexusData.Broker.Domain;

public interface IJob
{
    public Task ProcessAsync(Job jobRecord);
}