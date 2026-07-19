using System.ComponentModel;
using System.Text.Json;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Request Retention Reminder Scan")]
public class RequestRetentionReminderJob : IJob
{
    private static readonly int[] MilestoneDays = new[] { 14, 7, 3, 1 };

    private readonly JobStatusService<RequestRetentionReminderJob> _jobStatusService;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Job> _jobRepository;
    private readonly JobService _jobService;
    private readonly SettingsService _settingsService;

    public RequestRetentionReminderJob(
        JobStatusService<RequestRetentionReminderJob> jobStatusService,
        IRepository<Request> requestRepository,
        IRepository<Job> jobRepository,
        JobService jobService,
        SettingsService settingsService)
    {
        _jobStatusService = jobStatusService;
        _requestRepository = requestRepository;
        _jobRepository = jobRepository;
        _jobService = jobService;
        _settingsService = settingsService;
    }

    public async Task ProcessAsync(Job jobInstance)
    {
        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Begin retention reminder scan.");

        var cleanupDays = await _settingsService.GetRequestCleanupDaysAsync();
        var deletionCutoffDate = DateTimeOffset.UtcNow.AddDays(-cleanupDays);
        var reminderJobType = typeof(RequestRetentionReminderEmailJob).FullName!;

        // Applicable milestones, largest (furthest out) first, that actually fit inside the retention
        // window (e.g. a 7-day reminder is meaningless when everything is deleted after 3 days).
        var applicableMilestones = MilestoneDays
            .Where(days => days < cleanupDays)
            .OrderByDescending(days => days)
            .ToArray();

        var queuedCount = 0;

        for (var i = 0; i < applicableMilestones.Length; i++)
        {
            var daysRemaining = applicableMilestones[i];

            // A request stays inside a larger milestone's window until it also qualifies for the next
            // smaller one. Bounding each window below by the next milestone's cutoff (instead of the
            // constant deletion cutoff) keeps the windows disjoint, so a request that goes stale fast -
            // or is picked up by a delayed scan - gets only the single most urgent reminder that's
            // still accurate, not every milestone label it happened to blow past in one run.
            var nextMilestoneDays = i + 1 < applicableMilestones.Length ? applicableMilestones[i + 1] : (int?)null;
            var windowLowerBound = nextMilestoneDays is not null
                ? DateTimeOffset.UtcNow.AddDays(-(cleanupDays - nextMilestoneDays.Value))
                : deletionCutoffDate;

            var reminderCutoffDate = DateTimeOffset.UtcNow.AddDays(-(cleanupDays - daysRemaining));
            var dueRequests = await _requestRepository.ListAsync(new RequestsDueForRetentionReminder(reminderCutoffDate, windowLowerBound));

            foreach (var request in dueRequests)
            {
                // All milestones share one job type, so dedup by inspecting each existing reminder
                // job's parameters for this request rather than by job type alone.
                var existingReminderJobs = await _jobRepository.ListAsync(new JobsByReferenceAndType(reminderJobType, request.Id));
                var alreadyQueued = existingReminderJobs.Any(j => JobParametersMatch(j, daysRemaining));
                if (alreadyQueued) continue;

                var jobParameters = JsonSerializer.SerializeToDocument(new RetentionReminderJobParameters { DaysRemaining = daysRemaining });
                await _jobService.CreateJobAsync(typeof(RequestRetentionReminderEmailJob), typeof(Request), request.Id, null, jobParameters, request.EducationOrganizationId);
                queuedCount++;
            }
        }

        await _settingsService.SetValueAsync("LastRequestRetentionReminderRunAt", DateTimeOffset.UtcNow.ToString("O"));

        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Complete, "Retention reminder scan complete. Queued {0} reminder(s).", queuedCount);
    }

    private static bool JobParametersMatch(Job job, int daysRemaining)
    {
        if (job.JobParameters is null) return false;

        var parameters = JsonSerializer.Deserialize<RetentionReminderJobParameters>(job.JobParameters);
        return parameters?.DaysRemaining == daysRemaining;
    }
}
