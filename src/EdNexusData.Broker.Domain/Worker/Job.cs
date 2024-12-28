using System.Text.Json;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Domain.Worker;

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

    // public Core.Contracts.Job ToContract()
    // {
    //     var job = new Core.Contracts.Job()
    //     {
    //         QueueDateTime = this.QueueDateTime,
    //         StartDateTime = this.StartDateTime,
    //         FinishDateTime = this.FinishDateTime,
    //         JobType = this.JobType,
    //         JobParameters = this.JobParameters,
    //         ReferenceType = this.ReferenceType,
    //         ReferenceGuid = this.ReferenceGuid,
    //         JobStatus = this.JobStatus.ToContract(),
    //         WorkerInstance = this.WorkerInstance,
    //         WorkerState = this.WorkerState,
    //         WorkerLog = this.WorkerLog
    //     };

    //     return job;
    // }
}