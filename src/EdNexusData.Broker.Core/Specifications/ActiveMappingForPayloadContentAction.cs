using Ardalis.Specification;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications;

public class ActiveMappingForPayloadContentAction : Specification<PayloadContentAction>, ISingleResultSpecification
{
  public ActiveMappingForPayloadContentAction(Guid actionid)
  {
    Query
        .Include(x => x.ActiveMapping)
        .Where(x => x.Id == actionid);
  }
}