using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Worker;

public class JobStatusService<T>
{
    private readonly IRepository<Job> _jobRepo;
    private readonly IRepository<Request> _requestRepo;
    private readonly IRepository<Message> _messageRepo;
    private readonly IRepository<PayloadContentAction> _payloadContentActionRepo;
    private readonly JobStatusStore jobStatusStore;
    private readonly ILogger<T> _logger;

    public JobStatusService(ILogger<T> logger, 
           IRepository<Job> jobRepo,
           IRepository<Request> requestRepo,
           IRepository<Message> messageRepo,
           IRepository<PayloadContentAction> payloadContentActionRepo,
           JobStatusStore jobStatusStore)
    {
        _logger = logger;
        _jobRepo = jobRepo;
        _requestRepo = requestRepo;
        _payloadContentActionRepo = payloadContentActionRepo;
        _messageRepo = messageRepo;
        this.jobStatusStore = jobStatusStore;
    }

    public async Task<Job?> Get(Guid jobId)
    {
        return await _jobRepo.GetByIdAsync(jobId);
    }

    public async Task UpdateJobStatus(Job jobRecord, JobStatus? newJobStatus, string? message, params object?[] messagePlaceholders)
    {
        if (newJobStatus is not null) { jobRecord.JobStatus = newJobStatus.Value; }
        if (message is not null && messagePlaceholders is not null && messagePlaceholders.Count() > 0)
        { 
            jobRecord.WorkerState = string.Format(message, messagePlaceholders);
        }
        else
        {
            jobRecord.WorkerState = message;
        }

        if (!jobStatusStore.Logs.ContainsKey(jobRecord.Id))
        {
            jobStatusStore.Logs[jobRecord.Id] = "";
        }

        jobStatusStore.Logs[jobRecord.Id] += string.Format("{0}\t{1}\t{2}\r\n", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), Thread.CurrentThread.ManagedThreadId, jobRecord.WorkerState);

        jobRecord.JobStatus = newJobStatus!.Value;

        var endStatuses = new List<JobStatus> { JobStatus.Interrupted, JobStatus.Complete, JobStatus.Aborted, JobStatus.Failed };

        if (endStatuses.Contains(newJobStatus!.Value))
        {
            jobRecord.FinishDateTime = DateTime.UtcNow;
            jobRecord.WorkerLog = jobStatusStore.Logs[jobRecord.Id];
            jobStatusStore.Logs.Remove(jobRecord.Id);
        }
        
        await _jobRepo.UpdateAsync(jobRecord);

        _logger.LogInformation($"{jobRecord.Id}: {message}", messagePlaceholders!);
    }

    public async Task UpdateRequestStatus(Job jobRecord, Request request, RequestStatus? newRequestStatus, string? message, params object?[] messagePlaceholders)
    {
        if (newRequestStatus is not null) { request.RequestStatus = newRequestStatus.Value; }
        await _requestRepo.UpdateAsync(request);
        await UpdateJobStatus(jobRecord, JobStatus.Running, message, messagePlaceholders);

        _logger.LogInformation($"{jobRecord.Id} / {request.Id}: {message}", messagePlaceholders);
    }

    public async Task UpdateMessageStatus(Job jobRecord, Message message, RequestStatus? newRequestStatus, string? messageText, params object?[] messagePlaceholders)
    {
        if (newRequestStatus is not null) { message.RequestStatus = newRequestStatus.Value; }
        await _messageRepo.UpdateAsync(message);
        await UpdateJobStatus(jobRecord, JobStatus.Running, messageText, messagePlaceholders);

        _logger.LogInformation($"{jobRecord.Id} / {message.Id}: {message}", messagePlaceholders);
    }

    public async Task UpdatePayloadContentActionStatus(Job jobRecord, PayloadContentAction payloadContentAction, PayloadContentActionStatus? newPayloadContentActionStatus, string? message, params object?[] messagePlaceholders)
    {
        if (newPayloadContentActionStatus is not null) { payloadContentAction.PayloadContentActionStatus = newPayloadContentActionStatus.Value; }
        payloadContentAction.ProcessState = message;
        await _payloadContentActionRepo.UpdateAsync(payloadContentAction);
        await UpdateJobStatus(jobRecord, JobStatus.Running, message, messagePlaceholders);

        _logger.LogInformation($"{jobRecord.Id} / {payloadContentAction.Id}: {message}", messagePlaceholders);
    }

}