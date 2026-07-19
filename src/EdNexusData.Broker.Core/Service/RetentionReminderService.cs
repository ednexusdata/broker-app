using System.Text.Json;
using EdNexusData.Broker.Core.Emails.ViewModels;
using EdNexusData.Broker.Core.Jobs;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Specifications;

namespace EdNexusData.Broker.Core.Services;

public class RetentionReminderService
{
    private readonly IReadRepository<Request> _requestRepository;
    private readonly JobService _jobService;
    private readonly SettingsService _settingsService;

    public RetentionReminderService(
        IReadRepository<Request> requestRepository,
        JobService jobService,
        SettingsService settingsService)
    {
        _requestRepository = requestRepository;
        _jobService = jobService;
        _settingsService = settingsService;
    }

    /// <summary>Queues a retention reminder email for the given request, if it still exists and has a contact on file. Returns whether the email was queued.</summary>
    public async Task<bool> SendReminderAsync(Guid requestId, int daysRemaining)
    {
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(requestId));
        if (request is null) return false;

        var toEmail = request.EducationOrganization?.Contacts?.FirstOrDefault()?.Email;
        if (string.IsNullOrEmpty(toEmail)) return false;

        var cleanupDays = await _settingsService.GetRequestCleanupDaysAsync();
        var lastActivityAt = request.UpdatedAt ?? request.CreatedAt;
        var destructionAt = lastActivityAt.AddDays(cleanupDays);

        var student = request.RequestManifest?.Student ?? request.ResponseManifest?.Student;
        var studentDisplay = (student is not null) ? $"{student.FirstName} {student.LastName}".Trim() : "a request";

        var emailData = new EmailJobDetail
        {
            TemplateName = "RetentionReminder",
            To = toEmail,
            Subject = $"Records request for {studentDisplay} will be deleted in {daysRemaining} day{(daysRemaining == 1 ? "" : "s")}",
            Model = new RetentionReminderViewModel
            {
                Student = student,
                RequestId = request.Id,
                DaysRemaining = daysRemaining,
                DestructionAt = destructionAt
            },
            ModelType = typeof(RetentionReminderViewModel).FullName
        };

        await _jobService.CreateJobAsync(typeof(SendEmailJob), typeof(Request), request.Id, null, JsonSerializer.SerializeToDocument(emailData), request.EducationOrganizationId);

        return true;
    }

    /// <summary>
    /// Queues a permanent-deletion notice for a request about to be cleaned up, with its Proof of Request
    /// report attached as the only remaining record. The caller must supply an already-loaded request
    /// (with EducationOrganization included) since this runs immediately before the row is deleted.
    /// </summary>
    public async Task<bool> SendDestructionNotificationAsync(Request request, byte[] proofOfRequestPdf)
    {
        var toEmail = request.EducationOrganization?.Contacts?.FirstOrDefault()?.Email;
        if (string.IsNullOrEmpty(toEmail)) return false;

        var cleanupDays = await _settingsService.GetRequestCleanupDaysAsync();
        var lastActivityAt = request.UpdatedAt ?? request.CreatedAt;

        var student = request.RequestManifest?.Student ?? request.ResponseManifest?.Student;
        var studentDisplay = (student is not null) ? $"{student.FirstName} {student.LastName}".Trim() : "a request";

        var emailData = new EmailJobDetail
        {
            TemplateName = "RetentionDestroyed",
            To = toEmail,
            Subject = $"Records request for {studentDisplay} has been permanently deleted",
            Model = new RetentionDestroyedViewModel
            {
                Student = student,
                RequestId = request.Id,
                From = request.RequestManifest?.From,
                To = request.RequestManifest?.To,
                LastActivityAt = lastActivityAt,
                DestroyedAt = DateTimeOffset.UtcNow,
                RetentionDays = cleanupDays
            },
            ModelType = typeof(RetentionDestroyedViewModel).FullName,
            Attachments = new List<EmailAttachment>
            {
                new EmailAttachment
                {
                    FileName = $"ProofOfRequest-{request.Id}.pdf",
                    ContentType = "application/pdf",
                    Content = proofOfRequestPdf
                }
            }
        };

        await _jobService.CreateJobAsync(typeof(SendEmailJob), typeof(Request), request.Id, null, JsonSerializer.SerializeToDocument(emailData), request.EducationOrganizationId);

        return true;
    }
}
