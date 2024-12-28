// using EdNexusData.Broker.Core;
// using EdNexusData.Broker.Core.Worker;

// namespace EdNexusData.Broker.Core;

// public interface IJobStatusService
// {

//     public Task<Job?> Get(Guid? jobId);

//     public Task UpdateJobStatus(JobStatus? newJobStatus, string? message, params object?[] messagePlaceholders);

//     public Task UpdateRequestStatus(RequestStatus? newRequestStatus, string? message, params object?[] messagePlaceholders);

//     public Task UpdatePayloadContentActionStatus(PayloadContentActionStatus? newPayloadContentActionStatus, string? message, params object?[] messagePlaceholders);
// }