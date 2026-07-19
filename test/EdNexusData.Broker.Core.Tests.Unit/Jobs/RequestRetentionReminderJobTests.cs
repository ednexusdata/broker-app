using System.Text.Json;
using Ardalis.Specification;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Core.Worker;
using Microsoft.Extensions.Logging;
using Moq;

namespace EdNexusData.Broker.Core.Jobs.Tests.Unit;

public class RequestRetentionReminderJobTests
{
    private readonly Mock<IRepository<Request>> requestRepository = new();
    private readonly Mock<IRepository<Job>> jobRepository = new();
    private readonly Mock<IRepository<Message>> messageRepository = new();
    private readonly Mock<IRepository<PayloadContentAction>> payloadContentActionRepository = new();
    private readonly Mock<IRepository<Setting>> settingsWriteRepository = new();
    private readonly Mock<IReadRepository<Setting>> settingsReadRepository = new();

    private readonly List<Request> allRequests = new();
    private readonly List<Job> existingJobs = new();
    private readonly List<Job> queuedJobs = new();

    private readonly RequestRetentionReminderJob job;

    public RequestRetentionReminderJobTests()
    {
        requestRepository
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Request>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Request> spec, CancellationToken _) => ((Specification<Request>)spec).Evaluate(allRequests).ToList());

        jobRepository
            .Setup(r => r.ListAsync(It.IsAny<ISpecification<Job>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Job> spec, CancellationToken _) => ((Specification<Job>)spec).Evaluate(existingJobs).ToList());

        jobRepository
            .Setup(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .Callback<Job, CancellationToken>((j, _) =>
            {
                queuedJobs.Add(j);
                existingJobs.Add(j);
            })
            .ReturnsAsync((Job j, CancellationToken _) => j);

        var jobStatusService = new JobStatusService<RequestRetentionReminderJob>(
            new Mock<ILogger<RequestRetentionReminderJob>>().Object,
            jobRepository.Object,
            requestRepository.Object,
            messageRepository.Object,
            payloadContentActionRepository.Object,
            new JobStatusStore());

        var jobService = new JobService(jobRepository.Object);

        job = new RequestRetentionReminderJob(
            jobStatusService,
            requestRepository.Object,
            jobRepository.Object,
            jobService,
            new SettingsService(settingsWriteRepository.Object, settingsReadRepository.Object));
    }

    private void SetCleanupDays(int cleanupDays)
    {
        settingsReadRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Setting>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Setting { Key = "RequestCleanupDays", Value = cleanupDays.ToString() });
    }

    private static Request MakeRequest(DateTimeOffset lastActivityAt)
    {
        return new Request { Id = Guid.NewGuid(), CreatedAt = lastActivityAt, UpdatedAt = null, Payload = "Test" };
    }

    private static int? DaysRemainingOf(Job job)
    {
        if (job.JobParameters is null) return null;
        return JsonSerializer.Deserialize<RetentionReminderJobParameters>(job.JobParameters)?.DaysRemaining;
    }

    [Fact]
    public async Task ProcessAsync_OnlyAppliesMilestonesThatFitInsideTheRetentionWindow()
    {
        SetCleanupDays(3); // only the 1-day milestone fits inside a 3-day window
        var request = MakeRequest(DateTimeOffset.UtcNow.AddDays(-2.5)); // ~0.5 days remaining
        allRequests.Add(request);

        await job.ProcessAsync(new Job { Id = Guid.NewGuid() });

        var queued = Assert.Single(queuedJobs);
        Assert.Equal(typeof(RequestRetentionReminderEmailJob).FullName, queued.JobType);
        Assert.Equal(request.Id, queued.ReferenceGuid);
        Assert.Equal(1, DaysRemainingOf(queued));
    }

    [Fact]
    public async Task ProcessAsync_SendsOnlyOneReminder_ForARequestThatFellPastMultipleMilestonesAtOnce()
    {
        // A request that's gone stale (or was picked up by a delayed scan) can land inside what would
        // naively be several milestone windows simultaneously. It must get exactly one, accurate reminder.
        SetCleanupDays(15);
        var request = MakeRequest(DateTimeOffset.UtcNow.AddDays(-13)); // ~2 days actually remaining
        allRequests.Add(request);

        await job.ProcessAsync(new Job { Id = Guid.NewGuid() });

        var queued = Assert.Single(queuedJobs);
        Assert.Equal(request.Id, queued.ReferenceGuid);
        Assert.Equal(3, DaysRemainingOf(queued)); // the tightest milestone it currently qualifies for
    }

    [Fact]
    public async Task ProcessAsync_DoesNotRequeue_WhenThatMilestoneWasAlreadySentForTheRequest()
    {
        SetCleanupDays(15);
        var request = MakeRequest(DateTimeOffset.UtcNow.AddDays(-13)); // matches the 3-day milestone
        allRequests.Add(request);
        existingJobs.Add(new Job
        {
            Id = Guid.NewGuid(),
            JobType = typeof(RequestRetentionReminderEmailJob).FullName,
            ReferenceGuid = request.Id,
            JobParameters = JsonSerializer.SerializeToDocument(new RetentionReminderJobParameters { DaysRemaining = 3 })
        });

        await job.ProcessAsync(new Job { Id = Guid.NewGuid() });

        Assert.Empty(queuedJobs);
    }

    [Fact]
    public async Task ProcessAsync_StillQueues_WhenOnlyADifferentMilestoneWasPreviouslySent()
    {
        SetCleanupDays(15);
        var request = MakeRequest(DateTimeOffset.UtcNow.AddDays(-13)); // now due for the 3-day milestone
        allRequests.Add(request);
        existingJobs.Add(new Job
        {
            Id = Guid.NewGuid(),
            JobType = typeof(RequestRetentionReminderEmailJob).FullName,
            ReferenceGuid = request.Id,
            JobParameters = JsonSerializer.SerializeToDocument(new RetentionReminderJobParameters { DaysRemaining = 7 })
        });

        await job.ProcessAsync(new Job { Id = Guid.NewGuid() });

        var queued = Assert.Single(queuedJobs);
        Assert.Equal(3, DaysRemainingOf(queued));
    }

    [Fact]
    public async Task ProcessAsync_IgnoresRequestsNotYetDueForAnyMilestone()
    {
        SetCleanupDays(15);
        var request = MakeRequest(DateTimeOffset.UtcNow); // fresh activity, ~15 days remaining, past even the widest milestone
        allRequests.Add(request);

        await job.ProcessAsync(new Job { Id = Guid.NewGuid() });

        Assert.Empty(queuedJobs);
    }

    [Fact]
    public async Task ProcessAsync_QueuesTheWidestMilestone_AssoonAsItsCutoffIsReached()
    {
        SetCleanupDays(15);
        var request = MakeRequest(DateTimeOffset.UtcNow.AddDays(-1)); // exactly 14 days remaining
        allRequests.Add(request);

        await job.ProcessAsync(new Job { Id = Guid.NewGuid() });

        var queued = Assert.Single(queuedJobs);
        Assert.Equal(14, DaysRemainingOf(queued));
    }
}
