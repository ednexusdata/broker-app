using System.Text.Json;

namespace EdNexusData.Broker.Core;

public class ActivityLog : BaseEntity, IAggregateRoot
{
    // Null for system/worker-initiated activity (e.g. a background job acting on a request with no
    // interactive user involved).
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public ActivityType ActivityType { get; set; }

    // Stable machine key, e.g. "Login.Password", "Requests.Open", "Mapping.Import", "Jobs.RequestCleanup".
    public string Action { get; set; } = default!;

    // Human-readable text for the admin UI.
    public string? Description { get; set; }

    // Null for Login rows.
    public Guid? RequestId { get; set; }
    public Request? Request { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public JsonDocument? Metadata { get; set; }
}
