namespace EdNexusData.Broker.Web.Models.ActivityLogs;

public class ActivityLogViewModel
{
    public Guid Id { get; set; }
    public string CreatedAt { get; set; } = default!;
    public string User { get; set; } = default!;
    public ActivityType ActivityType { get; set; }
    public string Action { get; set; } = default!;
    public string? Description { get; set; }
    public Guid? RequestId { get; set; }
    public string? IpAddress { get; set; }

    public ActivityLogViewModel(ActivityLog log, TimeZoneInfo timezone)
    {
        Id = log.Id;
        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(log.CreatedAt.DateTime, timezone).ToString("M/dd/yyyy h:mm:ss tt");
        User = log.User?.LastFirstName ?? "System";
        ActivityType = log.ActivityType;
        Action = log.Action;
        Description = log.Description;
        RequestId = log.RequestId;
        IpAddress = log.IpAddress;
    }
}
