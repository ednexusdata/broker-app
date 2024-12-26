using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Specifications;

public class PayloadContentsWithRequest : Specification<PayloadContent>, ISingleResultSpecification
{
  public PayloadContentsWithRequest(Guid payloadContentId)
  {
    Query
        .Include(x => x.Request)
        .ThenInclude(x => x!.EducationOrganization)
        .ThenInclude(x => x!.ParentOrganization)
        .Where(x => x.Id == payloadContentId);
  }
}

