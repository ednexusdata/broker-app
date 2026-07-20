using Moq;

namespace EdNexusData.Broker.Core.Services.Tests.Unit;

public class ActivityLogServiceTests
{
    private readonly Mock<IRepository<ActivityLog>> activityLogRepository = new();
    private readonly ActivityLogService service;

    private ActivityLog? addedLog;

    public ActivityLogServiceTests()
    {
        activityLogRepository
            .Setup(r => r.AddAsync(It.IsAny<ActivityLog>(), It.IsAny<CancellationToken>()))
            .Callback<ActivityLog, CancellationToken>((log, _) => addedLog = log)
            .ReturnsAsync((ActivityLog log, CancellationToken _) => log);

        service = new ActivityLogService(activityLogRepository.Object);
    }

    [Fact]
    public async Task LogAsync_PersistsAllSuppliedFields()
    {
        var userId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        await service.LogAsync(
            ActivityType.RequestWork,
            userId,
            "Requests.Close",
            "Closed request",
            requestId,
            ip: "127.0.0.1",
            userAgent: "TestAgent/1.0");

        Assert.NotNull(addedLog);
        Assert.Equal(ActivityType.RequestWork, addedLog!.ActivityType);
        Assert.Equal(userId, addedLog.UserId);
        Assert.Equal("Requests.Close", addedLog.Action);
        Assert.Equal("Closed request", addedLog.Description);
        Assert.Equal(requestId, addedLog.RequestId);
        Assert.Equal("127.0.0.1", addedLog.IpAddress);
        Assert.Equal("TestAgent/1.0", addedLog.UserAgent);
    }

    [Fact]
    public async Task LogAsync_AllowsNullRequestId_ForLoginActivity()
    {
        var userId = Guid.NewGuid();

        await service.LogAsync(ActivityType.Login, userId, "Login.Password", "Signed in", requestId: null);

        Assert.NotNull(addedLog);
        Assert.Equal(ActivityType.Login, addedLog!.ActivityType);
        Assert.Null(addedLog.RequestId);
    }

    [Fact]
    public async Task LogAsync_AllowsNullUserId_ForSystemInitiatedActivity()
    {
        var requestId = Guid.NewGuid();

        await service.LogAsync(
            ActivityType.RequestWork,
            userId: null,
            "Jobs.RequestCleanup",
            "Request permanently deleted (retention period expired with no further activity)",
            requestId);

        Assert.NotNull(addedLog);
        Assert.Null(addedLog!.UserId);
        Assert.Equal("Jobs.RequestCleanup", addedLog.Action);
        Assert.Equal(requestId, addedLog.RequestId);
    }
}
