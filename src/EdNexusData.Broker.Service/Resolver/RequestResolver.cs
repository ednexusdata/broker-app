using Ardalis.GuardClauses;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Worker;
using EdNexusData.Broker.Service.Services;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Service.Resolvers;

public class RequestResolver
{
    private readonly ILogger<RequestResolver> logger;
    private readonly JobStatusService jobStatusService;
    private readonly IRepository<Request> requestRepository;

    public RequestResolver(
        ILogger<RequestResolver> logger,
        JobStatusService jobStatusService,
        IRepository<Request> requestRepository
        )
    {
        this.logger = logger;
        this.jobStatusService = jobStatusService;
        this.requestRepository = requestRepository;
    }

    public async Task<Request?> Resolve(Job jobInstance)
    {
        Guard.Against.Null(jobInstance.ReferenceGuid, "Request Guid", "Need request ID to resolve");
        
        jobStatusService.JobRecord = jobInstance;
        await jobStatusService.UpdateJobStatus(JobStatus.Running, "Resolving request {0}", jobInstance.ReferenceGuid);

        var requestId = jobInstance.ReferenceGuid;

        var request = await requestRepository.GetByIdAsync(requestId.Value);

        await jobStatusService.UpdateJobStatus(JobStatus.Running, "Resolved request {0}", request);

        return request;
    }
}