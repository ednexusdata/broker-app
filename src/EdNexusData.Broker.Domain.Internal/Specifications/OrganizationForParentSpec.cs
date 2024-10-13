using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Internal.Specifications;

public class OrganizationByParentSpec : Specification<EducationOrganization>, ISingleResultSpecification
{
  public OrganizationByParentSpec(Guid parentOrganizationId)
  {
    Query
        .Include(x => x.ParentOrganization)
        .Where(org => org.ParentOrganizationId == parentOrganizationId 
                   && org.EducationOrganizationType == EducationOrganizationType.School);
  }
}