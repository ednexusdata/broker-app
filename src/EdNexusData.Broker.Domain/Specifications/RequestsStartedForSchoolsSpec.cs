using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Specifications;

public class RequestsStartedForSchoolsSpec : Specification<Request>, ISingleResultSpecification
{
  public RequestsStartedForSchoolsSpec(List<EducationOrganization> focusedSchools, DateTime? startDate)
  {
    Query
        .Include(x => x.EducationOrganization)
        .Include(x => x.EducationOrganization!.ParentOrganization)
        .Include(x => x.PayloadContents)
        .Include(x => x.RequestProcessUser)
        .Include(x => x.ResponseProcessUser)
        .Where(request => (startDate == null || request.CreatedAt >= startDate)
            && focusedSchools.Contains(request.EducationOrganization!))
        .OrderByDescending(incomingRequest => incomingRequest.CreatedAt);
  }
}