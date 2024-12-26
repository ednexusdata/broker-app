using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Specifications;

public class MappingByActionId : Specification<Mapping>, ISingleResultSpecification
{
  public MappingByActionId(Guid actionId)
  {
    Query
        .Include(x => x.PayloadContentAction)
        .ThenInclude(x => x!.PayloadContent)
        .Where(req => req.PayloadContentActionId == actionId)
        .OrderBy(x => x.MappingType);
  }
}