using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class RequestByIdwithEdOrgs : Specification<Request>, ISingleResultSpecification
{
  public RequestByIdwithEdOrgs(Guid requestId)
  {
    Query
        .Include(x => x.EducationOrganization)
        .Include(x => x.EducationOrganization!.ParentOrganization)
        .Where(req => req.Id == requestId);
  }
}