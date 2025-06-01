using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class SettingByKeySpecification : Specification<Setting>
{
  public SettingByKeySpecification(string key)
  {
    Query.Where(setting => setting.Key == key);
  }
}