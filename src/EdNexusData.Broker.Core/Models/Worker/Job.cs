using System.Text.Json;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Worker;

public class Job : BaseEntity, IAggregateRoot
{
    public DateTimeOffset QueueDateTime { get; set; } = DateTime.UtcNow;
    public DateTimeOffset? StartDateTime { get; set; }
    public DateTimeOffset? FinishDateTime { get; set; }
    public string? JobType { get; set; }
    public JsonDocument? JobParameters { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceGuid { get; set; }
    public JobStatus JobStatus { get; set; } = JobStatus.Waiting;
    public string? WorkerInstance { get; set; }
    public string? WorkerState { get; set; }
    public string? WorkerLog { get; set; }

    public Guid? InitiatedUserId { get; set; }
    public User? InitiatedUser { get; set; }

    public Common.Worker.Job ToContract()
    {
        var job = new Common.Worker.Job()
        {
            QueueDateTime = QueueDateTime,
            StartDateTime = StartDateTime,
            FinishDateTime = FinishDateTime,
            JobType = JobType,
            JobParameters = JobParameters,
            ReferenceType = ReferenceType,
            ReferenceGuid = ReferenceGuid,
            JobStatus = JobStatus,
            WorkerInstance = WorkerInstance,
            WorkerState = WorkerState,
            WorkerLog = WorkerLog
        };

        return job;
    }
}