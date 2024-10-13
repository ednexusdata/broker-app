using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Internal.Specifications;

public class RequestsStartedForSchoolsSpec : Specification<Request>, ISingleResultSpecification
{
  public RequestsStartedForSchoolsSpec(List<EducationOrganization> focusedSchools, DateTime? startDate)
  {
    Query
        .Include(x => x.EducationOrganization)
        .Include(x => x.EducationOrganization!.ParentOrganization)
        .Include(x => x.PayloadContents)
        .Where(request => (startDate == null || request.CreatedAt >= startDate)
            && focusedSchools.Contains(request.EducationOrganization!))
        .OrderByDescending(incomingRequest => incomingRequest.CreatedAt);
  }
}