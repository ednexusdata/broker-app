using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Specifications.Tests.Unit;

public class RequestsPastRetentionTests
{
    private static Request MakeRequest(DateTimeOffset createdAt, DateTimeOffset? updatedAt, RequestStatus status = RequestStatus.InProgress)
    {
        return new Request
        {
            Id = Guid.NewGuid(),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            RequestStatus = status,
            Payload = "Test"
        };
    }

    [Fact]
    public void RequestsPastRetention_UsesUpdatedAt_WhenPresent()
    {
        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddDays(-15);

        var staleByUpdate = MakeRequest(createdAt: now.AddDays(-30), updatedAt: now.AddDays(-20));
        var freshByUpdate = MakeRequest(createdAt: now.AddDays(-30), updatedAt: now.AddDays(-1));

        var result = new RequestsPastRetention(cutoff).Evaluate(new List<Request> { staleByUpdate, freshByUpdate });

        Assert.Single(result);
        Assert.Equal(staleByUpdate.Id, result.First().Id);
    }

    [Fact]
    public void RequestsPastRetention_FallsBackToCreatedAt_WhenNeverUpdated()
    {
        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddDays(-15);

        var neverTouchedAndStale = MakeRequest(createdAt: now.AddDays(-20), updatedAt: null);
        var neverTouchedButFresh = MakeRequest(createdAt: now.AddDays(-1), updatedAt: null);

        var result = new RequestsPastRetention(cutoff).Evaluate(new List<Request> { neverTouchedAndStale, neverTouchedButFresh });

        Assert.Single(result);
        Assert.Equal(neverTouchedAndStale.Id, result.First().Id);
    }

    [Fact]
    public void RequestsPastRetention_AppliesRegardlessOfStatus()
    {
        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddDays(-15);
        var staleActivityAt = now.AddDays(-20);

        var draft = MakeRequest(createdAt: staleActivityAt, updatedAt: staleActivityAt, status: RequestStatus.Draft);
        var inProgress = MakeRequest(createdAt: staleActivityAt, updatedAt: staleActivityAt, status: RequestStatus.InProgress);
        var closed = MakeRequest(createdAt: staleActivityAt, updatedAt: staleActivityAt, status: RequestStatus.Closed);
        var finished = MakeRequest(createdAt: staleActivityAt, updatedAt: staleActivityAt, status: RequestStatus.Finished);

        var result = new RequestsPastRetention(cutoff).Evaluate(new List<Request> { draft, inProgress, closed, finished });

        Assert.Equal(4, result.Count());
    }
}
