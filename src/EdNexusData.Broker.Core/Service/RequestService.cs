using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Interfaces;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Services;

public class RequestService
{
    private readonly IRepository<Request> requestRepository;
    private readonly INowWrapper nowWrapper;
    private readonly JobStatusService<RequestService> jobStatusService;

    public RequestService(
        IRepository<Request> requestRepository,
        INowWrapper nowWrapper,
        JobStatusService<RequestService> jobStatusService
    )
    {
        this.requestRepository = requestRepository;
        this.nowWrapper = nowWrapper;
        this.jobStatusService = jobStatusService;
    }
    public async Task<Request?> Get(Job jobInstance)
    {
        _ = jobInstance.ReferenceGuid ?? throw new ArgumentNullException("Request ID required on reference ID for job.");
        
        await jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Resolving request {0}", jobInstance.ReferenceGuid);

        var request = await requestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(jobInstance.ReferenceGuid.Value));

        if (request is not null)
        {
            await jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Resolved request {0}", request?.Id);
        }
        
        return request;
    }

    public async Task<Request> MarkRequested(Request request, Job? jobInstance = null)
    {
        var dbRequest = await requestRepository.GetByIdAsync(request.Id);
        dbRequest!.InitialRequestSentDate = nowWrapper.UtcNow;
        await requestRepository.UpdateAsync(dbRequest);

        if (jobInstance is not null)
        {
            await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Requested, "Marked request as requested.");
        }

        return dbRequest;
    }
}