using Ardalis.Specification;
using EdNexusData.Broker.Common.EducationOrganizations;

namespace EdNexusData.Broker.Core.Specifications;

public class OrganizationWithChildSpec : Specification<EducationOrganization>, ISingleResultSpecification
{
  public OrganizationWithChildSpec(Guid organizationId)
  {
    Query
        .Include(x => x.ParentOrganization)
        .Include(x => x.EducationOrganizations)
        .Where(org => org.Id == organizationId);
  }
}