using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Jobs;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core;

public class JobStatusServiceProxy<T> : IJobStatusService
{
    private JobStatusService<T> jobStatusService;
    private PayloadContentAction? payloadContentAction;
    private Job job;
    private Request request;

    public JobStatusServiceProxy(JobStatusService<T> jobStatusService, Job job, Request request)
    {
        this.jobStatusService = jobStatusService;
        this.job = job;
        this.request = request;
    }

    public JobStatusServiceProxy(
        JobStatusService<T> jobStatusService,
        Job job,
        PayloadContentAction payloadContentAction,
        Request request
    )
    {
        this.jobStatusService = jobStatusService;
        this.job = job;
        this.payloadContentAction = payloadContentAction;
        this.request = request;
    }

    public async Task UpdateJobStatus(JobStatus? newJobStatus, string? message, params object?[] messagePlaceholders)
    {
        await jobStatusService.UpdateJobStatus(job, newJobStatus, message, messagePlaceholders);
    }

    public async Task UpdatePayloadContentActionStatus(
        Common.PayloadContentActions.PayloadContentActionStatus? newPayloadContentActionStatus,
        string? message,
        params object?[] messagePlaceholders
    )
    {
        _ = payloadContentAction ?? throw new ArgumentNullException(nameof(payloadContentAction), "PayloadContentAction must be provided.");

        PayloadContentActionStatus? convertedNewPayloadContentActionStatus = null;

        if (newPayloadContentActionStatus is not null)
        {
            convertedNewPayloadContentActionStatus = newPayloadContentActionStatus.Value switch
            {
                Common.PayloadContentActions.PayloadContentActionStatus.Error => PayloadContentActionStatus.Error,
                Common.PayloadContentActions.PayloadContentActionStatus.Imported => PayloadContentActionStatus.Imported,
                Common.PayloadContentActions.PayloadContentActionStatus.Importing => PayloadContentActionStatus.Importing,
                Common.PayloadContentActions.PayloadContentActionStatus.Mapped => PayloadContentActionStatus.Mapped,
                Common.PayloadContentActions.PayloadContentActionStatus.Prepared => PayloadContentActionStatus.Prepared,
                Common.PayloadContentActions.PayloadContentActionStatus.Preparing => PayloadContentActionStatus.Preparing,
                Common.PayloadContentActions.PayloadContentActionStatus.Ready => PayloadContentActionStatus.Ready,
                _ => throw new ArgumentOutOfRangeException(nameof(newPayloadContentActionStatus), "Invalid payload content action status.")
            };
        }

        await jobStatusService.UpdatePayloadContentActionStatus(
            job,
            payloadContentAction,
            convertedNewPayloadContentActionStatus,
            message,
            messagePlaceholders
        );
    }

    public async Task UpdateRequestStatus(
        RequestStatus? newRequestStatus,
        string? message,
        params object?[] messagePlaceholders)
    {
        await jobStatusService.UpdateRequestStatus(
            job,
            request,
            newRequestStatus,
            message,
            messagePlaceholders
        );
    }
}