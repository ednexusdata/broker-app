using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications.Tests.Unit;

public class JobsByReferenceAndTypeTests
{
    [Fact]
    public void JobsByReferenceAndType_MatchesOnJobTypeAndReferenceGuid()
    {
        var requestId = Guid.NewGuid();
        const string jobType = "EdNexusData.Broker.Core.Jobs.RequestRetentionReminderEmailJob";

        var matching = new Job { Id = Guid.NewGuid(), JobType = jobType, ReferenceGuid = requestId };
        var wrongType = new Job { Id = Guid.NewGuid(), JobType = "SomeOtherJob", ReferenceGuid = requestId };
        var wrongReference = new Job { Id = Guid.NewGuid(), JobType = jobType, ReferenceGuid = Guid.NewGuid() };

        var result = new JobsByReferenceAndType(jobType, requestId).Evaluate(new List<Job> { matching, wrongType, wrongReference });

        Assert.Single(result);
        Assert.Equal(matching.Id, result.First().Id);
    }

    [Fact]
    public void JobsByReferenceAndType_ReturnsAllMatchesForRepeatedMilestones()
    {
        var requestId = Guid.NewGuid();
        const string jobType = "EdNexusData.Broker.Core.Jobs.RequestRetentionReminderEmailJob";

        var sevenDay = new Job { Id = Guid.NewGuid(), JobType = jobType, ReferenceGuid = requestId };
        var threeDay = new Job { Id = Guid.NewGuid(), JobType = jobType, ReferenceGuid = requestId };

        var result = new JobsByReferenceAndType(jobType, requestId).Evaluate(new List<Job> { sevenDay, threeDay });

        Assert.Equal(2, result.Count());
    }
}
