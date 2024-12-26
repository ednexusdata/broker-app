// using EdNexusData.Broker.Domain;
// using EdNexusData.Broker.Domain.Worker;

// namespace EdNexusData.Broker.Service;

// public interface IJobStatusService
// {

//     public Task<Job?> Get(Guid? jobId);

//     public Task UpdateJobStatus(JobStatus? newJobStatus, string? message, params object?[] messagePlaceholders);

//     public Task UpdateRequestStatus(RequestStatus? newRequestStatus, string? message, params object?[] messagePlaceholders);

//     public Task UpdatePayloadContentActionStatus(PayloadContentActionStatus? newPayloadContentActionStatus, string? message, params object?[] messagePlaceholders);
// }