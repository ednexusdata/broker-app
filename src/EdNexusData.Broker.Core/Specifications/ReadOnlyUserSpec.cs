using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class ReadOnlyUserSpec : Specification<User>
{
  public ReadOnlyUserSpec(Guid id)
  {
    Query
        .Include(x => x.UserRoles!)
        .ThenInclude(x => x.EducationOrganization)
        .Where(i => i.Id == id)
        .AsNoTracking();
  }
}