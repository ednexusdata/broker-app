using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Internal.Specifications;

public class ConnectorByNameAndDistrictNumberSpec : Specification<EducationOrganizationConnectorSettings>
{
  public ConnectorByNameAndDistrictNumberSpec(string connectorName, string districtNumber)
  {
    Query
        .Include(x => x.EducationOrganization)
        .Where(x => x.Connector == connectorName && x.EducationOrganization!.Number == districtNumber)
        .EnableCache(nameof(ConnectorByNameAndDistrictNumberSpec), connectorName, districtNumber);
  }
}