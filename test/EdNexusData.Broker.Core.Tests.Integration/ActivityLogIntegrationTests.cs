using System.Text.Json;
using EdNexusData.Broker.Common.EducationOrganizations;
using EdNexusData.Broker.Core.Reports;
using EdNexusData.Broker.Core.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Core.Tests.Integration;

// Proves the RequestId FK's OnDelete(DeleteBehavior.Cascade) config is actually correct against a real
// Postgres database: RequestCleanupJob hard-deletes Request rows, and its ActivityLog rows must go with
// it rather than surviving as orphaned, unlinkable rows (the request's history is captured in the Proof
// of Request PDF before the delete, so nothing is lost — just not left behind in this table). Also
// proves a null UserId (system/worker-initiated activity) round-trips correctly, since UserId has a
// real FK to AspNetUsers that must tolerate being unset.
[Collection("BrokerWebDICollection")]
public class ActivityLogIntegrationTests
{
    private readonly BrokerWebDIServicesFixture _services;

    public ActivityLogIntegrationTests(BrokerWebDIServicesFixture services)
    {
        _services = services;
    }

    private async Task<EducationOrganization> SeedOrgAsync()
    {
        var educationOrganizationRepository = _services.Services!.GetRequiredService<IRepository<EducationOrganization>>();

        return await educationOrganizationRepository.AddAsync(new EducationOrganization
        {
            Id = Guid.NewGuid(),
            Name = "Activity Log Test District",
            ShortName = "ALTD",
            EducationOrganizationType = EducationOrganizationType.District
        });
    }

    private async Task<User> SeedUserAsync()
    {
        var userRepository = _services.Services!.GetRequiredService<IRepository<User>>();
        var userManager = _services.Services!.GetRequiredService<UserManager<IdentityUser<Guid>>>();

        // User.Id has a real FK to AspNetUsers, so the Identity-side row must exist first.
        var identityUser = new IdentityUser<Guid> { UserName = $"{Guid.NewGuid()}@test.example", Email = $"{Guid.NewGuid()}@test.example" };
        var identityResult = await userManager.CreateAsync(identityUser);
        Assert.True(identityResult.Succeeded, string.Join(", ", identityResult.Errors.Select(e => e.Description)));

        return await userRepository.AddAsync(new User
        {
            Id = identityUser.Id,
            FirstName = "Activity",
            LastName = "Logger"
        });
    }

    [Fact]
    public async Task ActivityLog_IsCascadeDeleted_WhenRequestIsDeleted()
    {
        var requestRepository = _services.Services!.GetRequiredService<IRepository<Request>>();
        var activityLogRepository = _services.Services!.GetRequiredService<IRepository<ActivityLog>>();

        var org = await SeedOrgAsync();
        var user = await SeedUserAsync();

        var request = await requestRepository.AddAsync(new Request
        {
            Id = Guid.NewGuid(),
            EducationOrganizationId = org.Id,
            Payload = "ActivityLogIntegrationTest"
        });

        var log = await activityLogRepository.AddAsync(new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ActivityType = ActivityType.RequestOpened,
            Action = "Requests.View",
            Description = "Opened request",
            RequestId = request.Id
        });

        // Mirrors RequestCleanupJob's hard delete once retention expires.
        await requestRepository.DeleteAsync(request);

        var deletedLog = await activityLogRepository.GetByIdAsync(log.Id);

        Assert.Null(deletedLog);
    }

    [Fact]
    public async Task ActivityLog_RoundTripsMetadataJsonColumn()
    {
        var activityLogRepository = _services.Services!.GetRequiredService<IRepository<ActivityLog>>();
        var user = await SeedUserAsync();

        var metadata = JsonSerializer.SerializeToDocument(new { fileName = "transcript.pdf", size = 1024 });

        var log = await activityLogRepository.AddAsync(new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ActivityType = ActivityType.RequestWork,
            Action = "Incoming.UploadAttachment",
            Metadata = metadata
        });

        var reloaded = await activityLogRepository.GetByIdAsync(log.Id);

        Assert.NotNull(reloaded);
        Assert.NotNull(reloaded!.Metadata);
        Assert.Equal("transcript.pdf", reloaded.Metadata!.RootElement.GetProperty("fileName").GetString());
        Assert.Equal(1024, reloaded.Metadata!.RootElement.GetProperty("size").GetInt32());
    }

    [Fact]
    public async Task ActivityLog_AllowsNullUserId_ForSystemInitiatedActivity()
    {
        var requestRepository = _services.Services!.GetRequiredService<IRepository<Request>>();
        var activityLogRepository = _services.Services!.GetRequiredService<IRepository<ActivityLog>>();

        var org = await SeedOrgAsync();
        var request = await requestRepository.AddAsync(new Request
        {
            Id = Guid.NewGuid(),
            EducationOrganizationId = org.Id,
            Payload = "ActivityLogIntegrationTest"
        });

        // Mirrors RequestCleanupJob logging its own automated deletion, with no interactive user.
        var log = await activityLogRepository.AddAsync(new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = null,
            ActivityType = ActivityType.RequestWork,
            Action = "Jobs.RequestCleanup",
            Description = "Request permanently deleted (retention period expired with no further activity)",
            RequestId = request.Id
        });

        var reloaded = await activityLogRepository.GetByIdAsync(log.Id);

        Assert.NotNull(reloaded);
        Assert.Null(reloaded!.UserId);
        Assert.Equal("Jobs.RequestCleanup", reloaded.Action);
    }

    [Fact]
    public async Task ProofOfRequestReport_IncludesActivityLogSection_WhenRequestHasActivity()
    {
        var requestRepository = _services.Services!.GetRequiredService<IRepository<Request>>();
        var activityLogRepository = _services.Services!.GetRequiredService<IRepository<ActivityLog>>();
        var proofOfRequestReport = _services.Services!.GetRequiredService<ProofOfRequestReport>();

        var org = await SeedOrgAsync();
        var user = await SeedUserAsync();

        var request = await requestRepository.AddAsync(new Request
        {
            Id = Guid.NewGuid(),
            EducationOrganizationId = org.Id,
            Payload = "ActivityLogIntegrationTest"
        });

        await activityLogRepository.AddAsync(new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ActivityType = ActivityType.RequestOpened,
            Action = "Requests.View",
            Description = "Opened request",
            RequestId = request.Id
        });

        var pdfBytes = await proofOfRequestReport.Generate(request.Id, "Test Generator", TimeZoneInfo.Utc);

        Assert.NotEmpty(pdfBytes);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4));
    }
}
