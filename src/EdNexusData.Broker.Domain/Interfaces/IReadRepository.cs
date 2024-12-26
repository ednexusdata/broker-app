using Ardalis.Specification;

namespace EdNexusData.Broker.Domain;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot
{
    
}