using EdNexusData.Broker.Common.EducationOrganizations;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Core.Tests.Integration.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Core.Tests.Integration;

// These exercise the retention/reminder specifications through IRepository<Request> against a real
// Postgres database, so they catch EF-to-SQL translation issues (Npgsql's handling of DateTimeOffset
// comparisons, the UpdatedAt ?? CreatedAt coalesce, etc.) that an in-memory .Evaluate() test cannot.
[Collection("BrokerWebDICollection")]
public class RetentionSpecificationsIntegrationTests
{
    private readonly BrokerWebDIServicesFixture _services;
    private EducationOrganization? _educationOrganization;

    public RetentionSpecificationsIntegrationTests(BrokerWebDIServicesFixture services)
    {
        _services = services;
    }

    private async Task<EducationOrganization> GetOrCreateEducationOrganizationAsync()
    {
        if (_educationOrganization is not null) return _educationOrganization;

        var educationOrganizationRepository = _services.Services!.GetRequiredService<IRepository<EducationOrganization>>();
        _educationOrganization = await educationOrganizationRepository.AddAsync(new EducationOrganization
        {
            Id = Guid.NewGuid(),
            Name = "Retention Spec Test District",
            ShortName = "RSTD",
            EducationOrganizationType = EducationOrganizationType.District
        });

        return _educationOrganization;
    }

    // Bypasses EfRepository (which always stamps CreatedAt/UpdatedAt to "now") so tests can plant
    // requests with a controlled, backdated last-activity time.
    private async Task<Request> InsertRequestWithActivityAtAsync(DateTimeOffset lastActivityAt, RequestStatus status = RequestStatus.InProgress)
    {
        var educationOrganization = await GetOrCreateEducationOrganizationAsync();
        var dbContext = _services.Services!.GetRequiredService<DbContext>();

        var request = new Request
        {
            Id = Guid.NewGuid(),
            EducationOrganizationId = educationOrganization.Id,
            Payload = "RetentionSpecIntegrationTest",
            RequestStatus = status,
            CreatedAt = lastActivityAt,
            UpdatedAt = null
        };

        dbContext.Set<Request>().Add(request);
        await dbContext.SaveChangesAsync();

        return request;
    }

    [Fact]
    public async Task RequestsPastRetention_FindsStaleRequests_RegardlessOfStatus()
    {
        var requestRepository = _services.Services!.GetRequiredService<IRepository<Request>>();
        var cutoff = DateTimeOffset.UtcNow.AddDays(-15);

        var staleClosed = await InsertRequestWithActivityAtAsync(DateTimeOffset.UtcNow.AddDays(-20), RequestStatus.Closed);
        var staleInProgress = await InsertRequestWithActivityAtAsync(DateTimeOffset.UtcNow.AddDays(-20), RequestStatus.InProgress);
        var fresh = await InsertRequestWithActivityAtAsync(DateTimeOffset.UtcNow.AddDays(-1), RequestStatus.InProgress);

        var result = await requestRepository.ListAsync(new RequestsPastRetention(cutoff));
        var resultIds = result.Select(r => r.Id).ToHashSet();

        Assert.Contains(staleClosed.Id, resultIds);
        Assert.Contains(staleInProgress.Id, resultIds);
        Assert.DoesNotContain(fresh.Id, resultIds);
    }

    [Fact]
    public async Task RequestsDueForRetentionReminder_RespectsWindowBounds()
    {
        var requestRepository = _services.Services!.GetRequiredService<IRepository<Request>>();
        var now = DateTimeOffset.UtcNow;
        var reminderCutoff = now.AddDays(-8);
        var deletionCutoff = now.AddDays(-15);

        var dueForReminder = await InsertRequestWithActivityAtAsync(now.AddDays(-9));
        var alreadyPastDeletion = await InsertRequestWithActivityAtAsync(now.AddDays(-20));
        var tooFresh = await InsertRequestWithActivityAtAsync(now.AddDays(-2));

        var result = await requestRepository.ListAsync(new RequestsDueForRetentionReminder(reminderCutoff, deletionCutoff));
        var resultIds = result.Select(r => r.Id).ToHashSet();

        Assert.Contains(dueForReminder.Id, resultIds);
        Assert.DoesNotContain(alreadyPastDeletion.Id, resultIds);
        Assert.DoesNotContain(tooFresh.Id, resultIds);
    }
}
