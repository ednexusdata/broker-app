using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class EducationOrganizationDomainsByValuesSpec : Specification<EducationOrganizationDomain>
{
  public EducationOrganizationDomainsByValuesSpec(IEnumerable<string> domains)
  {
    Query
        .Where(d => domains.Contains(d.Domain));
  }
}
