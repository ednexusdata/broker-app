// using System.ComponentModel;
// using EdNexusData.Broker.Core.Contracts;

// namespace EdNexusData.Broker.Domain.Worker;

// public enum JobStatus
// {
//     [Description("Waiting")]
//     Waiting,
    
//     [Description("Running")]
//     Running,

//     [Description("Complete")]
//     Complete,

//     [Description("Aborted")]
//     Aborted,

//     [Description("Interrupted")]
//     Interrupted,

//     [Description("Failed")]
//     Failed
// }

// public static class JobStatusExtensions
// {
//     public static Core.Jobs.JobStatus ToContract(this JobStatus value)
//     {
//         return (Core.Jobs.JobStatus) Enum.Parse(typeof(JobStatus), value.ToString());
//     }
// }