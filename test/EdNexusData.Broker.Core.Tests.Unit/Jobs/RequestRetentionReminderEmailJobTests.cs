using System.Text.Json;
using Ardalis.Specification;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Worker;
using Microsoft.Extensions.Logging;
using Moq;

namespace EdNexusData.Broker.Core.Jobs.Tests.Unit;

public class RequestRetentionReminderEmailJobTests
{
    private readonly Mock<IReadRepository<Request>> requestReadRepository = new();
    private readonly Mock<IRepository<Request>> requestRepository = new();
    private readonly Mock<IRepository<Job>> jobRepository = new();
    private readonly Mock<IRepository<Message>> messageRepository = new();
    private readonly Mock<IRepository<PayloadContentAction>> payloadContentActionRepository = new();
    private readonly Mock<IRepository<Setting>> settingsWriteRepository = new();
    private readonly Mock<IReadRepository<Setting>> settingsReadRepository = new();

    private readonly List<Job> queuedJobs = new();
    private readonly RequestRetentionReminderEmailJob job;

    public RequestRetentionReminderEmailJobTests()
    {
        settingsReadRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Setting>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Setting { Key = "RequestCleanupDays", Value = "15" });

        jobRepository
            .Setup(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .Callback<Job, CancellationToken>((j, _) => queuedJobs.Add(j))
            .ReturnsAsync((Job j, CancellationToken _) => j);

        var settingsService = new SettingsService(settingsWriteRepository.Object, settingsReadRepository.Object);
        var jobService = new JobService(jobRepository.Object);
        var retentionReminderService = new RetentionReminderService(requestReadRepository.Object, jobService, settingsService);

        var jobStatusService = new JobStatusService<RequestRetentionReminderEmailJob>(
            new Mock<ILogger<RequestRetentionReminderEmailJob>>().Object,
            jobRepository.Object,
            requestRepository.Object,
            messageRepository.Object,
            payloadContentActionRepository.Object,
            new JobStatusStore());

        job = new RequestRetentionReminderEmailJob(jobStatusService, retentionReminderService);
    }

    private static Request MakeRequestWithContact(string email)
    {
        return new Request
        {
            Id = Guid.NewGuid(),
            Payload = "Test",
            EducationOrganization = new EducationOrganization
            {
                Id = Guid.NewGuid(),
                Name = "Test District",
                ShortName = "TD",
                Contacts = new List<EducationOrganizationContact> { new() { Name = "Front Office", Email = email } }
            }
        };
    }

    [Fact]
    public async Task ProcessAsync_ParsesDaysRemainingFromParameters_AndQueuesTheEmail()
    {
        var request = MakeRequestWithContact("frontoffice@testdistrict.example");
        requestReadRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Request>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var jobInstance = new Job
        {
            Id = Guid.NewGuid(),
            ReferenceGuid = request.Id,
            JobParameters = JsonSerializer.SerializeToDocument(new RetentionReminderJobParameters { DaysRemaining = 3 })
        };

        await job.ProcessAsync(jobInstance);

        var queued = Assert.Single(queuedJobs);
        Assert.Equal(typeof(SendEmailJob).FullName, queued.JobType);
        var emailDetail = JsonSerializer.Deserialize<EmailJobDetail>(queued.JobParameters!);
        Assert.Contains("3 day", emailDetail!.Subject);
        Assert.Equal(JobStatus.Complete, jobInstance.JobStatus);
    }

    [Fact]
    public async Task ProcessAsync_CompletesWithoutQueuing_WhenNoContactEmailOnFile()
    {
        var request = new Request { Id = Guid.NewGuid(), Payload = "Test", EducationOrganization = new EducationOrganization { Id = Guid.NewGuid(), Name = "No Contact District", ShortName = "NCD" } };
        requestReadRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Request>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var jobInstance = new Job
        {
            Id = Guid.NewGuid(),
            ReferenceGuid = request.Id,
            JobParameters = JsonSerializer.SerializeToDocument(new RetentionReminderJobParameters { DaysRemaining = 1 })
        };

        await job.ProcessAsync(jobInstance);

        Assert.Empty(queuedJobs);
        Assert.Equal(JobStatus.Complete, jobInstance.JobStatus);
    }

    [Fact]
    public async Task ProcessAsync_Throws_WhenReferenceGuidMissing()
    {
        var jobInstance = new Job
        {
            Id = Guid.NewGuid(),
            ReferenceGuid = null,
            JobParameters = JsonSerializer.SerializeToDocument(new RetentionReminderJobParameters { DaysRemaining = 1 })
        };

        await Assert.ThrowsAsync<ArgumentNullException>(() => job.ProcessAsync(jobInstance));
    }

    [Fact]
    public async Task ProcessAsync_Throws_WhenJobParametersMissing()
    {
        var jobInstance = new Job
        {
            Id = Guid.NewGuid(),
            ReferenceGuid = Guid.NewGuid(),
            JobParameters = null
        };

        await Assert.ThrowsAsync<ArgumentNullException>(() => job.ProcessAsync(jobInstance));
    }
}
