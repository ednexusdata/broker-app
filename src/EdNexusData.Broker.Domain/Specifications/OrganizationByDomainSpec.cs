using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Specifications;

public class OrganizationByDomainSpec : Specification<EducationOrganization>, ISingleResultSpecification
{
  
  public OrganizationByDomainSpec(string domain)
  {
    Query
        .Include(edOrg => edOrg.EducationOrganizations!.OrderBy(x => x.Name))
        .Where(edOrg => edOrg.Domain == domain)
        .EnableCache(nameof(OrganizationByDomainSpec), domain);
  }
}