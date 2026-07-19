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

        var queuedCount = 0;

        foreach (var daysRemaining in MilestoneDays)
        {
            // If the retention window is shorter than this milestone, it can never apply (e.g. a 7-day
            // reminder is meaningless when everything is deleted after 3 days).
            if (daysRemaining >= cleanupDays) continue;

            var reminderCutoffDate = DateTimeOffset.UtcNow.AddDays(-(cleanupDays - daysRemaining));
            var dueRequests = await _requestRepository.ListAsync(new RequestsDueForRetentionReminder(reminderCutoffDate, deletionCutoffDate));

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
