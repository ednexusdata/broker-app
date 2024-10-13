using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Internal.Specifications;

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