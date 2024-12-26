using EdNexusData.Broker.Core.Jobs;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Worker;

namespace EdNexusData.Broker.Service;

public class JobStatusServiceProxy : IJobStatusService
{
    private PayloadJob payloadJob;
    private Job job;
    private Request request;

    public JobStatusServiceProxy(PayloadJob payloadJob, Job job, Request request)
    {
        this.payloadJob = payloadJob;
        this.job = job;
        this.request = request;
    }

    public Task UpdateJobStatus(JobStatus? newJobStatus, string? message, params object?[] messagePlaceholders)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePayloadContentActionStatus(Core.PayloadContentActions.PayloadContentActionStatus? newPayloadContentActionStatus, string? message, params object?[] messagePlaceholders)
    {
        throw new NotImplementedException();
    }

    public Task UpdateRequestStatus(RequestStatus? newRequestStatus, string? message, params object?[] messagePlaceholders)
    {
        throw new NotImplementedException();
    }
}