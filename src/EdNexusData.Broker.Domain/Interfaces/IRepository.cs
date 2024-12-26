using Ardalis.Specification;

namespace EdNexusData.Broker.Domain;

public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
}