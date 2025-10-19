using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class SettingsSortByKeySpecification : Specification<Setting>
{
  public SettingsSortByKeySpecification()
  {
    Query.OrderBy(setting => setting.Key);
  }
}