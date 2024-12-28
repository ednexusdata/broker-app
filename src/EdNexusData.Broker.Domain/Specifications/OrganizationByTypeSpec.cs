using Ardalis.Specification;
using EdNexusData.Broker.Common.EducationOrganizations;

namespace EdNexusData.Broker.Domain.Specifications;

public class OrganizationByTypeSpec : Specification<EducationOrganization>, ISingleResultSpecification
{
  public OrganizationByTypeSpec(EducationOrganizationType orgType)
  {
    Query
        .Include(x => x.ParentOrganization)
        .Where(organizationType => organizationType.EducationOrganizationType == orgType)
        .EnableCache(nameof(OrganizationByTypeSpec), orgType);
  }
}