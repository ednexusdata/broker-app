using Ardalis.Specification;
using EdNexusData.Broker.Domain.Worker;

namespace EdNexusData.Broker.Domain.Internal.Specifications;

public class ActiveMappingForPayloadContentAction : Specification<PayloadContentAction>, ISingleResultSpecification
{
  public ActiveMappingForPayloadContentAction(Guid actionid)
  {
    Query
        .Include(x => x.ActiveMapping)
        .Where(x => x.Id == actionid);
  }
}