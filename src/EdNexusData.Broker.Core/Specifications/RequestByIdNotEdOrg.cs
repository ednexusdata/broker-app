using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class RequestByIdNotEdOrg : Specification<Request>, ISingleResultSpecification
{
  public RequestByIdNotEdOrg(Guid requestId, Guid educationOrganizationId)
  {
    Query
        .Where(req => 
          req.Id == requestId && req.EducationOrganizationId != educationOrganizationId
        );
  }
}