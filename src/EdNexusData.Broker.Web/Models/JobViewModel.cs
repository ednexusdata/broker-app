using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Domain.Worker;

namespace EdNexusData.Broker.Web.Models;

public class JobViewModel
{
    public Guid? JobId { get; set; }

    public string QueuedDateTime { get; set; } = default!;

    public string? StartDateTime { get; set; }

    public string? FinishDateTime { get; set; }

    public JobStatus? JobStatus { get; set; }

    public string? WorkerState { get; set; }

    public string? WorkerInstance { get; set; }

    public string? JobType { get; set; }

    public string? User { get; set; }

    public JobViewModel(Job job, TimeZoneInfo timezone)
    {
        JobId = job.Id;

        QueuedDateTime = TimeZoneInfo.ConvertTimeFromUtc(job.QueueDateTime.DateTime, timezone).ToString("M/dd/yyyy h:mm:ss tt");

        StartDateTime = (job.StartDateTime is not null) 
            ? TimeZoneInfo.ConvertTimeFromUtc(job.StartDateTime.Value.DateTime, timezone).ToString("M/dd/yyyy h:mm:ss tt") 
            : null;
        
        FinishDateTime = (job.FinishDateTime is not null) 
            ? TimeZoneInfo.ConvertTimeFromUtc(job.FinishDateTime.Value.DateTime, timezone).ToString("M/dd/yyyy h:mm:ss tt") 
            : null;

        JobStatus = job.JobStatus;
        WorkerInstance = job.WorkerInstance;
        if (job.WorkerState?.Length > 50)
        {
            WorkerState = job.WorkerState?.Substring(0, 50) + "...";
        }
        else
        {
            WorkerState = job.WorkerState;
        }
        
        JobType = job.JobType;

        User = job.CreatedByUser?.LastFirstName;
    }
}