using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Interfaces;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Core.Worker;
using Microsoft.EntityFrameworkCore.Storage.Json;

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

    public async Task<Request?> Get(Guid requestId)
    {
        return await requestRepository.GetByIdAsync(requestId);
    }

    public async Task<Request?> UpdateStatus(Request request, RequestStatus requestStatus)
    {
        var latestRequest = await requestRepository.GetByIdAsync(request.Id);
        _ = latestRequest ?? throw new NullReferenceException("Unable to find request");

        latestRequest.RequestStatus = requestStatus;
        await requestRepository.UpdateAsync(latestRequest);

        return latestRequest;
    }

    public async Task<Request> Create(
        Guid educationOrganizationId, 
        Manifest manifest, 
        RequestStatus requestStatus, 
        IncomingOutgoing incomingOutgoing)
    {
        var request = new Request()
        {
            EducationOrganizationId = educationOrganizationId,
            RequestStatus = requestStatus,
            IncomingOutgoing = incomingOutgoing,
            Payload = manifest.RequestType
        };

        if (incomingOutgoing == IncomingOutgoing.Incoming)
        {
            request.ResponseManifest = manifest;
        } else if (incomingOutgoing == IncomingOutgoing.Outgoing)
        {
            request.RequestManifest = manifest;
        }

        await requestRepository.AddAsync(request);

        return request;
    }

    public async Task<Request> UpdateResponseManifest(Guid requestId, Manifest manifest)
    {
        var request = await requestRepository.GetByIdAsync(requestId);
        _ = request ?? throw new NullReferenceException($"Unable to find request Id {requestId}");
        
        request.ResponseManifest = manifest;
        await requestRepository.UpdateAsync(request);
        
        return request;
    }

    public async Task<bool> Close(Guid requestId)
    {
        var request = await requestRepository.GetByIdAsync(requestId);
        _ = request ?? throw new NullReferenceException($"Unable to find request Id {requestId}");

        request.Open = false;
        await requestRepository.UpdateAsync(request);

        await jobStatusService.UpdateRequestStatus(request, RequestStatus.Finished, "Request closed from requester");

        return true;
    }

    public async Task<bool> Open(Guid requestId)
    {
        var request = await requestRepository.GetByIdAsync(requestId);
        _ = request ?? throw new NullReferenceException($"Unable to find request Id {requestId}");

        request.Open = true;
        await requestRepository.UpdateAsync(request);

        return true;
    }
}