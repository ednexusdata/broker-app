using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class UserRolesByUserSpec : Specification<UserRole>
{
  public UserRolesByUserSpec(Guid UserId)
  {
    Query
        .Include(x => x.EducationOrganization)
        .ThenInclude(y => y!.ParentOrganization)
        .Where(user => user.UserId == UserId);
  }
}