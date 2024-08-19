using Microsoft.Extensions.Logging;
using System.Linq;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Domain.Worker;
using Ardalis.GuardClauses;

namespace EdNexusData.Broker.Service.Worker;

public class JobStatusService<T>
{
    private readonly IRepository<Job> _jobRepo;
    private readonly IRepository<Request> _requestRepo;
    private readonly IRepository<PayloadContentAction> _payloadContentActionRepo;
    private readonly ILogger<T> _logger;

    public JobStatusService(ILogger<T> logger, 
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

    public async Task UpdateJobStatus(Job jobRecord, JobStatus? newJobStatus, string? message, params object?[] messagePlaceholders)
    {
        if (newJobStatus is not null) { jobRecord.JobStatus = newJobStatus.Value; }
        if (message is not null) { jobRecord.WorkerState = string.Format(message, messagePlaceholders); }
        jobRecord.JobStatus = newJobStatus!.Value;

        var endStatuses = new List<JobStatus> { JobStatus.Interrupted, JobStatus.Complete, JobStatus.Aborted, JobStatus.Failed };

        if (endStatuses.Contains(newJobStatus!.Value))
        {
            jobRecord.FinishDateTime = DateTime.UtcNow;
        }
        
        await _jobRepo.UpdateAsync(jobRecord);

        _logger.LogInformation($"{jobRecord.Id}: {message}", messagePlaceholders);
    }

    public async Task UpdateRequestStatus(Job jobRecord, Request request, RequestStatus? newRequestStatus, string? message, params object?[] messagePlaceholders)
    {
        if (newRequestStatus is not null) { request.RequestStatus = newRequestStatus.Value; }
        request.ProcessState = message;
        await _requestRepo.UpdateAsync(request);
        await UpdateJobStatus(jobRecord, JobStatus.Running, message, messagePlaceholders);

        _logger.LogInformation($"{jobRecord.Id} / {request.Id}: {message}", messagePlaceholders);
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