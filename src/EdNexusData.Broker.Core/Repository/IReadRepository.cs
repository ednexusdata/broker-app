using Ardalis.Specification;

namespace EdNexusData.Broker.Core;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot
{
    
}