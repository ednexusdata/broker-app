using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class MappingWithPayloadContent : Specification<Mapping>, ISingleResultSpecification
{
  public MappingWithPayloadContent(Guid id)
  {
    Query
        .Include(x => x.PayloadContentAction)
        .ThenInclude(x => x!.PayloadContent)
        .ThenInclude(x => x!.Request)
        .ThenInclude(x => x!.EducationOrganization)
        .Where(x => x.Id == id);
  }
}