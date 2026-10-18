using System.Text.Json;
using Ardalis.Specification;
using EdNexusData.Broker.Core.Emails.ViewModels;
using EdNexusData.Broker.Core.Jobs;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Worker;
using Moq;

namespace EdNexusData.Broker.Core.Services.Tests.Unit;

public class RetentionReminderServiceTests
{
    private readonly Mock<IReadRepository<Request>> requestRepository = new();
    private readonly Mock<IRepository<Job>> jobRepository = new();
    private readonly Mock<IRepository<Setting>> settingsWriteRepository = new();
    private readonly Mock<IReadRepository<Setting>> settingsReadRepository = new();
    private readonly RetentionReminderService service;

    private Job? addedJob;

    public RetentionReminderServiceTests()
    {
        settingsReadRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Setting>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Setting { Key = "RequestCleanupDays", Value = "15" });

        jobRepository
            .Setup(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .Callback<Job, CancellationToken>((job, _) => addedJob = job)
            .ReturnsAsync((Job job, CancellationToken _) => job);

        var jobService = new JobService(jobRepository.Object);
        var settingsService = new SettingsService(settingsWriteRepository.Object, settingsReadRepository.Object);

        service = new RetentionReminderService(requestRepository.Object, jobService, settingsService);
    }

    private static Request MakeRequestWithContact(string? email, DateTimeOffset lastActivityAt)
    {
        return new Request
        {
            Id = Guid.NewGuid(),
            EducationOrganizationId = Guid.NewGuid(),
            CreatedAt = lastActivityAt,
            UpdatedAt = null,
            Payload = "Test",
            RequestManifest = new Manifest
            {
                RequestType = "Test",
                Student = new Student { FirstName = "Jamie", LastName = "Rivera", StudentNumber = "12345" }
            },
            EducationOrganization = new EducationOrganization
            {
                Id = Guid.NewGuid(),
                Name = "Test District",
                ShortName = "TD",
                Contacts = email is null ? null : new List<EducationOrganizationContact>
                {
                    new EducationOrganizationContact { Name = "Front Office", Email = email }
                }
            }
        };
    }

    [Fact]
    public async Task SendReminderAsync_ReturnsFalse_WhenRequestNotFound()
    {
        requestRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Request>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Request?)null);

        var sent = await service.SendReminderAsync(Guid.NewGuid(), 7);

        Assert.False(sent);
        jobRepository.Verify(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendReminderAsync_ReturnsFalse_WhenNoContactEmailOnFile()
    {
        var request = MakeRequestWithContact(email: null, lastActivityAt: DateTimeOffset.UtcNow.AddDays(-8));
        requestRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Request>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var sent = await service.SendReminderAsync(request.Id, 7);

        Assert.False(sent);
        jobRepository.Verify(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendReminderAsync_QueuesSendEmailJob_WithExpectedContent()
    {
        var lastActivityAt = DateTimeOffset.UtcNow.AddDays(-8);
        var request = MakeRequestWithContact(email: "frontoffice@testdistrict.example", lastActivityAt);
        requestRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<Request>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var sent = await service.SendReminderAsync(request.Id, 7);

        Assert.True(sent);
        Assert.NotNull(addedJob);
        Assert.Equal(typeof(SendEmailJob).FullName, addedJob!.JobType);
        Assert.Equal(request.Id, addedJob.ReferenceGuid);

        var emailDetail = JsonSerializer.Deserialize<EmailJobDetail>(addedJob.JobParameters!);
        Assert.NotNull(emailDetail);
        Assert.Equal("RetentionReminder", emailDetail!.TemplateName);
        Assert.Equal("frontoffice@testdistrict.example", emailDetail.To);
        Assert.Contains("7 day", emailDetail.Subject);

        var model = JsonSerializer.Deserialize<RetentionReminderViewModel>(((JsonElement)emailDetail.Model!).GetRawText());
        Assert.NotNull(model);
        Assert.Equal(request.Id, model!.RequestId);
        Assert.Equal(7, model.DaysRemaining);
        Assert.Equal(lastActivityAt.AddDays(15), model.DestructionAt);
        Assert.Equal("Jamie", model.Student?.FirstName);
    }

    [Fact]
    public async Task SendDestructionNotificationAsync_ReturnsFalse_WhenNoContactEmailOnFile()
    {
        var request = MakeRequestWithContact(email: null, lastActivityAt: DateTimeOffset.UtcNow.AddDays(-16));

        var sent = await service.SendDestructionNotificationAsync(request, new byte[] { 1, 2, 3 });

        Assert.False(sent);
        jobRepository.Verify(r => r.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendDestructionNotificationAsync_QueuesSendEmailJob_WithProofOfRequestAttached()
    {
        var lastActivityAt = DateTimeOffset.UtcNow.AddDays(-16);
        var request = MakeRequestWithContact(email: "frontoffice@testdistrict.example", lastActivityAt);
        var pdfBytes = new byte[] { 1, 2, 3, 4 };

        var sent = await service.SendDestructionNotificationAsync(request, pdfBytes);

        Assert.True(sent);
        Assert.NotNull(addedJob);
        Assert.Equal(typeof(SendEmailJob).FullName, addedJob!.JobType);

        var emailDetail = JsonSerializer.Deserialize<EmailJobDetail>(addedJob.JobParameters!);
        Assert.NotNull(emailDetail);
        Assert.Equal("RetentionDestroyed", emailDetail!.TemplateName);
        Assert.Equal("frontoffice@testdistrict.example", emailDetail.To);

        Assert.NotNull(emailDetail.Attachments);
        var attachment = Assert.Single(emailDetail.Attachments!);
        Assert.Equal("application/pdf", attachment.ContentType);
        Assert.Equal(pdfBytes, attachment.Content);
        Assert.Contains(request.Id.ToString(), attachment.FileName);

        var model = JsonSerializer.Deserialize<RetentionDestroyedViewModel>(((JsonElement)emailDetail.Model!).GetRawText());
        Assert.NotNull(model);
        Assert.Equal(request.Id, model!.RequestId);
        Assert.Equal(lastActivityAt, model.LastActivityAt);
        Assert.Equal(15, model.RetentionDays);
    }
}
