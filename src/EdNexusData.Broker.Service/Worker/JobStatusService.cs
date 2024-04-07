using Microsoft.Extensions.Logging;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;

namespace EdNexusData.Broker.Service.Worker;

public class JobStatusService<T>
{
    private readonly IRepository<Request> _requestsRepo;

    private readonly ILogger<T> _logger;

    public JobStatusService(ILogger<T> logger, IRepository<Request> requestsRepo)
    {
        _logger = logger;
        _requestsRepo = requestsRepo;
    }

    public async Task UpdateRequestJobStatus(Request request, RequestStatus? newRequestStatus, string? message, params object?[] messagePlaceholders)
    {
        if (newRequestStatus is not null) { request.RequestStatus = newRequestStatus.Value; }
        request.ProcessState = message;
        await _requestsRepo.UpdateAsync(request);

        _logger.LogInformation($"{request.Id}: {message}", messagePlaceholders);
    }

}