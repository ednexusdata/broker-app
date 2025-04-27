using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class EnabledConnectorsByEdOrgSpec : Specification<EducationOrganizationConnectorSettings>
{
  public EnabledConnectorsByEdOrgSpec(Guid educationOrganizationId)
  {
    Query
        .Where(x => 
               x.EducationOrganizationId == educationOrganizationId
            && x.Enabled == true)
        .EnableCache(nameof(EnabledConnectorsByEdOrgSpec), educationOrganizationId);
  }
}