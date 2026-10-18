using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class ActivityLogsByRequestId : Specification<ActivityLog>
{
    public ActivityLogsByRequestId(Guid requestId)
    {
        Query
            .Include(x => x.User)
            .Where(x => x.RequestId == requestId)
            .OrderBy(x => x.CreatedAt);
    }
}
