using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Resolvers;

public class RequestResolver
{
    private readonly JobStatusService<RequestResolver> jobStatusService;
    private readonly IReadRepository<Request> requestRepository;

    public RequestResolver(
        JobStatusService<RequestResolver> jobStatusService,
        IReadRepository<Request> requestRepository
    )
    {
        this.jobStatusService = jobStatusService;
        this.requestRepository = requestRepository;
    }

    public async Task<Request?> Resolve(Job jobInstance)
    {
        _ = jobInstance.ReferenceGuid ?? throw new ArgumentNullException("Request ID required on reference ID for job.");
        
        await jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Resolving request {0}", jobInstance.ReferenceGuid);

        var request = await requestRepository.GetByIdAsync(jobInstance.ReferenceGuid.Value);

        if (request is not null)
        {
            await jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Resolved request {0}", request?.Id);
        }
        
        return request;
    }
}