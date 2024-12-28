using Microsoft.Extensions.Logging;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Worker;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Service.Services;

public class JobStatusService
{
    private readonly IRepository<Job> _jobRepo;
    private readonly IRepository<Request> _requestRepo;
    private readonly IRepository<PayloadContentAction> _payloadContentActionRepo;
    private readonly ILogger _logger;

    public Job JobRecord { get; internal set; } = new Job();
    public Request RequestRecord { get; internal set; } = new Request();
    public PayloadContentAction PayloadContentActionRecord { get; internal set; } = new PayloadContentAction();

    public JobStatusService(ILogger logger, 
           IRepository<Job> jobRepo,
           IRepository<Request> requestRepo,
           IRepository<PayloadContentAction> payloadContentActionRepo)
    {
        _logger = logger;
        _jobRepo = jobRepo;
        _requestRepo = requestRepo;
        _payloadContentActionRepo = payloadContentActionRepo;
    }

    public async Task<Job?> Get(Guid? jobId)
    {
        Guard.Against.Null(jobId, "jobId", "Job id missing");
        
        return await _jobRepo.GetByIdAsync(jobId.Value);
    }

    public async Task UpdateJobStatus(JobStatus? newJobStatus, string? message, params object?[] messagePlaceholders)
    {
        if (newJobStatus is not null) { JobRecord.JobStatus = newJobStatus.Value; }
        if (message is not null && messagePlaceholders is not null && messagePlaceholders.Count() > 0)
        { 
            JobRecord.WorkerState = string.Format(message, messagePlaceholders);
        }
        else
        {
            JobRecord.WorkerState = message;
        }
        JobRecord.JobStatus = newJobStatus!.Value;

        var endStatuses = new List<JobStatus> { JobStatus.Interrupted, JobStatus.Complete, JobStatus.Aborted, JobStatus.Failed };

        if (endStatuses.Contains(newJobStatus!.Value))
        {
            JobRecord.FinishDateTime = DateTime.UtcNow;
        }
        
        await _jobRepo.UpdateAsync(JobRecord);

        _logger.LogInformation($"{JobRecord.Id}: {message}", messagePlaceholders!);
    }

    public async Task UpdateRequestStatus(RequestStatus? newRequestStatus, string? message, params object?[] messagePlaceholders)
    {
        if (newRequestStatus is not null) { RequestRecord.RequestStatus = newRequestStatus.Value; }
        await _requestRepo.UpdateAsync(RequestRecord);
        await UpdateJobStatus(JobStatus.Running, message, messagePlaceholders);

        _logger.LogInformation($"{JobRecord.Id} / {RequestRecord.Id}: {message}", messagePlaceholders);
    }

    public async Task UpdatePayloadContentActionStatus(PayloadContentActionStatus? newPayloadContentActionStatus, string? message, params object?[] messagePlaceholders)
    {
        if (newPayloadContentActionStatus is not null) { PayloadContentActionRecord.PayloadContentActionStatus = newPayloadContentActionStatus.Value; }
        PayloadContentActionRecord.ProcessState = message;
        await _payloadContentActionRepo.UpdateAsync(PayloadContentActionRecord);
        await UpdateJobStatus(JobStatus.Running, message, messagePlaceholders);

        _logger.LogInformation($"{JobRecord.Id} / {PayloadContentActionRecord.Id}: {message}", messagePlaceholders);
    }

}