using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class UserWithUserRolesByUserSpec : Specification<User>
{
  public UserWithUserRolesByUserSpec(Guid UserId)
  {
    Query
        .Include(x => x.UserRoles)!
        .ThenInclude(x => x.EducationOrganization)
        .Where(user => user.Id == UserId);
  }
}