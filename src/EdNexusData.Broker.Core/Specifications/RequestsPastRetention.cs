using Ardalis.Specification;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Specifications;

public class RequestsPastRetention : Specification<Request>
{
    public RequestsPastRetention(DateTimeOffset cutoffDate)
    {
        // Applies to a request in any status: inactivity (no activity since the cutoff),
        // not the request's current status, determines whether it is due for cleanup.
        // EducationOrganization is included to resolve a contact for the destruction notice email.
        Query
            .Include(r => r.EducationOrganization)
            .Where(r => (r.UpdatedAt ?? r.CreatedAt) <= cutoffDate);
    }
}
