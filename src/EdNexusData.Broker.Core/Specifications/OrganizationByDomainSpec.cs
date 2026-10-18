using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class OrganizationByDomainSpec : Specification<EducationOrganization>, ISingleResultSpecification
{
  
  public OrganizationByDomainSpec(string domain)
  {
    Query
        .Include(edOrg => edOrg.EducationOrganizations!.OrderBy(x => x.Name))
        .Include(edOrg => edOrg.Domains)
        .Where(edOrg => edOrg.Domains!.Any(d => d.Domain == domain))
        .EnableCache(nameof(OrganizationByDomainSpec), domain);
  }
}