using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class UserRolesByUserSpec : Specification<UserRole>
{
  public UserRolesByUserSpec(Guid UserId)
  {
    Query
        .Include(x => x.EducationOrganization)
        .Where(user => user.UserId == UserId);
  }
}