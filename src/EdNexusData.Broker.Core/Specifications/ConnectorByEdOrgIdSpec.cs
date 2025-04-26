using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class ConnectorByEdOrgIdSpec : Specification<EducationOrganizationConnectorSettings>
{
  public ConnectorByEdOrgIdSpec(Guid educationOrganizationId)
  {
    Query
        .Where(x => x.EducationOrganizationId == educationOrganizationId)
        .EnableCache(nameof(ConnectorByNameAndEdOrgIdSpec), educationOrganizationId);
  }
}