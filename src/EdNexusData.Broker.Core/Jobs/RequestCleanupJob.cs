using System.ComponentModel;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Reports;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Request Cleanup")]
public class RequestCleanupJob : IJob
{
    public const int DefaultCleanupDays = 30;
    private const string GeneratedByLabel = "System (Automated Retention Cleanup)";

    private readonly JobStatusService<RequestCleanupJob> _jobStatusService;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<PayloadContentAction> _actionRepository;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly SettingsService _settingsService;
    private readonly RetentionReminderService _retentionReminderService;
    private readonly ProofOfRequestReport _proofOfRequestReport;

    public RequestCleanupJob(
        JobStatusService<RequestCleanupJob> jobStatusService,
        IRepository<Request> requestRepository,
        IRepository<PayloadContent> payloadContentRepository,
        IRepository<PayloadContentAction> actionRepository,
        IRepository<Mapping> mappingRepository,
        SettingsService settingsService,
        RetentionReminderService retentionReminderService,
        ProofOfRequestReport proofOfRequestReport)
    {
        _jobStatusService = jobStatusService;
        _requestRepository = requestRepository;
        _payloadContentRepository = payloadContentRepository;
        _actionRepository = actionRepository;
        _mappingRepository = mappingRepository;
        _settingsService = settingsService;
        _retentionReminderService = retentionReminderService;
        _proofOfRequestReport = proofOfRequestReport;
    }

    public async Task ProcessAsync(Job jobInstance)
    {
        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Begin request cleanup.");

        var cleanupDays = await _settingsService.GetRequestCleanupDaysAsync();

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-cleanupDays);

        var expiredRequests = await _requestRepository.ListAsync(new RequestsPastRetention(cutoffDate));

        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Found {0} request(s) for cleanup.", expiredRequests.Count);

        foreach (var request in expiredRequests)
        {
            await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Processing request {0}.", request.Id);

            var payloadContents = await _payloadContentRepository.ListAsync(new PayloadContentsByRequestIdAll(request.Id));

            foreach (var payloadContent in payloadContents)
            {
                var actions = await _actionRepository.ListAsync(new PayloadContentActionsByPayloadContentId(payloadContent.Id));

                foreach (var action in actions)
                {
                    var mappings = await _mappingRepository.ListAsync(new MappingsByPayloadContentActionId(action.Id));

                    foreach (var mapping in mappings)
                    {
                        mapping.PayloadContentActionId = null;
                        await _mappingRepository.UpdateAsync(mapping);
                    }

                    action.ActiveMappingId = null;
                    await _actionRepository.UpdateAsync(action);
                }
            }

            // Generate the Proof of Request report and email it out before the row disappears for good;
            // it becomes the only surviving record of this request.
            var proofOfRequestPdf = await _proofOfRequestReport.Generate(request.Id, GeneratedByLabel, TimeZoneInfo.Utc);
            await _retentionReminderService.SendDestructionNotificationAsync(request, proofOfRequestPdf);

            await _requestRepository.DeleteAsync(request);

            await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Deleted request {0}.", request.Id);
        }

        await _settingsService.SetValueAsync("LastRequestCleanupRunAt", DateTimeOffset.UtcNow.ToString("O"));

        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Complete, "Request cleanup complete. Processed {0} request(s).", expiredRequests.Count);
    }
}
