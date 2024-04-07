using EdNexusData.Broker.Domain;
using Microsoft.Extensions.DependencyInjection;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Service.Jobs;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Service.Resolvers;

public class WorkerResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkerResolver> _logger;

    public WorkerResolver(IServiceProvider serviceProvider, ILogger<WorkerResolver> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Request> ProcessAsync(Request request)
    {
        Guard.Against.Null(request);
        
        using (var scoped = _serviceProvider.CreateScope())
        {
            //_logger.LogInformation("Start worker scope.");
            // Figure out which job to execute based on the state of the request
            switch (request.RequestStatus)
            {
                case RequestStatus.WaitingToSend:
                    var sendRequest = (SendRequest)scoped.ServiceProvider.GetService(typeof(SendRequest))!;
                    await sendRequest.Process(request);
                    break;
                case RequestStatus.WaitingToLoad:
                    var payloadContentLoader = (PayloadContentLoader)scoped.ServiceProvider.GetService(typeof(PayloadContentLoader))!;
                    await payloadContentLoader.Process(request);
                    break;
                case RequestStatus.WaitingToPrepare:
                    var prepareMappingLoader = (PrepareMapping)scoped.ServiceProvider.GetService(typeof(PrepareMapping))!;
                    await prepareMappingLoader.Process(request);
                    break;
                case RequestStatus.WaitingToImport:
                    var importMappingLoader = (ImportMapping)scoped.ServiceProvider.GetService(typeof(ImportMapping))!;
                    await importMappingLoader.Process(request);
                    break;
            }
            //_logger.LogInformation("End worker scope.");
        }
        
        return request;
    }
}