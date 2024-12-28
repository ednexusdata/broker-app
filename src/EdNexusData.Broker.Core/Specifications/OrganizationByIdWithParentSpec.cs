using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class OrganizationByIdWithParentSpec : Specification<EducationOrganization>, ISingleResultSpecification
{
  
  public OrganizationByIdWithParentSpec(Guid educationOrganizationId)
  {
    Query
        .Include(edOrg => edOrg.ParentOrganization)
        .Where(edOrg => edOrg.Id == educationOrganizationId)
        .EnableCache(nameof(OrganizationByIdWithParentSpec), educationOrganizationId);
  }
}