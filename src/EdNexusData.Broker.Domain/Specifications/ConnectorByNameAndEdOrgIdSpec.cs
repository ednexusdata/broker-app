using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Specifications;

public class ConnectorByNameAndEdOrgIdSpec : Specification<EducationOrganizationConnectorSettings>
{
  public ConnectorByNameAndEdOrgIdSpec(string connectorName, Guid educationOrganizationId)
  {
    Query
        .Where(x => x.Connector == connectorName && x.EducationOrganizationId == educationOrganizationId)
        .EnableCache(nameof(ConnectorByNameAndEdOrgIdSpec), connectorName, educationOrganizationId);
  }
}