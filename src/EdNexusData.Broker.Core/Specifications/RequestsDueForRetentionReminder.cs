using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class RequestsDueForRetentionReminder : Specification<Request>
{
    public RequestsDueForRetentionReminder(DateTimeOffset reminderCutoffDate, DateTimeOffset deletionCutoffDate)
    {
        // Requests inside the reminder window (last activity old enough that deletion is approaching)
        // but not yet past the actual deletion cutoff, which RequestCleanupJob handles separately.
        Query
            .Where(r => (r.UpdatedAt ?? r.CreatedAt) <= reminderCutoffDate && (r.UpdatedAt ?? r.CreatedAt) > deletionCutoffDate);
    }
}
