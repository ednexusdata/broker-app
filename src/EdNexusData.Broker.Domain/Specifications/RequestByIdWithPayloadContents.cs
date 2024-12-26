using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Specifications;

public class RequestByIdWithPayloadContents : Specification<Request>, ISingleResultSpecification
{
  public RequestByIdWithPayloadContents(Guid requestId)
  {
    Query
        .Include(x => x.EducationOrganization)
        .Include(x => x.EducationOrganization!.ParentOrganization)
        .Include(x => x.PayloadContents)
        .Where(req => req.Id == requestId);
  }
}