using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class MappingByRequestId : Specification<Mapping>, ISingleResultSpecification
{
  public MappingByRequestId(Guid RequestId)
  {
    Query
        .Include(x => x.PayloadContentAction)
        .ThenInclude(x => x!.PayloadContent)
        .ThenInclude(x => x!.Request)
        .Where(y => y.PayloadContentAction!.PayloadContent!.RequestId == RequestId)
        .OrderBy(x => x.MappingType);
  }
}