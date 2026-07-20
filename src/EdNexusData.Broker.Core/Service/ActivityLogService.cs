using System.Text.Json;

namespace EdNexusData.Broker.Core.Services;

public class ActivityLogService
{
    private readonly IRepository<ActivityLog> _activityLogRepository;

    public ActivityLogService(IRepository<ActivityLog> activityLogRepository)
    {
        _activityLogRepository = activityLogRepository;
    }

    // userId is null for system/worker-initiated activity (e.g. a background job acting on a request
    // with no interactive user involved).
    public async Task LogAsync(
        ActivityType activityType,
        Guid? userId,
        string action,
        string? description,
        Guid? requestId,
        string? ip = null,
        string? userAgent = null,
        JsonDocument? metadata = null)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            ActivityType = activityType,
            Action = action,
            Description = description,
            RequestId = requestId,
            IpAddress = ip,
            UserAgent = userAgent,
            Metadata = metadata
        };

        await _activityLogRepository.AddAsync(log);
    }
}
