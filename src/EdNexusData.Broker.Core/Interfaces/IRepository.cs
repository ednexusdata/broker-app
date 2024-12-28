using Ardalis.Specification;

namespace EdNexusData.Broker.Core;

public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
}