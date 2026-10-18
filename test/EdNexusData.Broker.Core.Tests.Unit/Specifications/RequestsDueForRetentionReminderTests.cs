namespace EdNexusData.Broker.Core.Specifications.Tests.Unit;

public class RequestsDueForRetentionReminderTests
{
    private static Request MakeRequest(DateTimeOffset lastActivityAt)
    {
        return new Request
        {
            Id = Guid.NewGuid(),
            CreatedAt = lastActivityAt,
            UpdatedAt = null,
            Payload = "Test"
        };
    }

    // Mirrors RequestRetentionReminderJob: given a 15-day retention window and a 7-day-out milestone,
    // the reminder cutoff is 8 days ago (15 - 7) and the deletion cutoff is 15 days ago.
    [Fact]
    public void RequestsDueForRetentionReminder_IncludesRequestsInsideWindow()
    {
        var now = DateTimeOffset.UtcNow;
        var reminderCutoff = now.AddDays(-8);
        var deletionCutoff = now.AddDays(-15);

        var dueForReminder = MakeRequest(now.AddDays(-9)); // older than reminder cutoff, not yet past deletion cutoff

        var result = new RequestsDueForRetentionReminder(reminderCutoff, deletionCutoff).Evaluate(new List<Request> { dueForReminder });

        Assert.Single(result);
    }

    [Fact]
    public void RequestsDueForRetentionReminder_ExcludesRequestsNotYetInWindow()
    {
        var now = DateTimeOffset.UtcNow;
        var reminderCutoff = now.AddDays(-8);
        var deletionCutoff = now.AddDays(-15);

        var tooRecent = MakeRequest(now.AddDays(-3)); // last activity more recent than the reminder cutoff

        var result = new RequestsDueForRetentionReminder(reminderCutoff, deletionCutoff).Evaluate(new List<Request> { tooRecent });

        Assert.Empty(result);
    }

    [Fact]
    public void RequestsDueForRetentionReminder_ExcludesRequestsAlreadyPastDeletionCutoff()
    {
        var now = DateTimeOffset.UtcNow;
        var reminderCutoff = now.AddDays(-8);
        var deletionCutoff = now.AddDays(-15);

        // Already due for actual cleanup; RequestCleanupJob owns this, not the reminder scan.
        var alreadyExpired = MakeRequest(now.AddDays(-20));

        var result = new RequestsDueForRetentionReminder(reminderCutoff, deletionCutoff).Evaluate(new List<Request> { alreadyExpired });

        Assert.Empty(result);
    }
}
