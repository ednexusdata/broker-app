using System.ComponentModel;
using System.Text.Json;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Request Retention Reminder Email")]
public class RequestRetentionReminderEmailJob : IJob
{
    private readonly JobStatusService<RequestRetentionReminderEmailJob> _jobStatusService;
    private readonly RetentionReminderService _retentionReminderService;

    public RequestRetentionReminderEmailJob(
        JobStatusService<RequestRetentionReminderEmailJob> jobStatusService,
        RetentionReminderService retentionReminderService)
    {
        _jobStatusService = jobStatusService;
        _retentionReminderService = retentionReminderService;
    }

    public async Task ProcessAsync(Job jobInstance)
    {
        _ = jobInstance.ReferenceGuid ?? throw new ArgumentNullException("Missing request reference on reminder job.");
        _ = jobInstance.JobParameters ?? throw new ArgumentNullException("Missing job parameters on reminder job.");

        var parameters = JsonSerializer.Deserialize<RetentionReminderJobParameters>(jobInstance.JobParameters);
        _ = parameters ?? throw new ArgumentNullException("Unable to deserialize reminder job parameters.");

        var sent = await _retentionReminderService.SendReminderAsync(jobInstance.ReferenceGuid.Value, parameters.DaysRemaining);

        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Complete,
            sent
                ? "Queued {0}-day retention reminder email."
                : "Skipped reminder: request or contact email not found.",
            parameters.DaysRemaining);
    }
}
