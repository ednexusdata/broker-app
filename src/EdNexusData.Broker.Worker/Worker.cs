using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.Service.Resolvers;

namespace EdNexusData.Broker.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);

            using (var scoped = _serviceProvider.CreateScope())
            {
                //_logger.LogInformation("Start scope.");
                var _requestsRepository = (IRepository<Request>)scoped.ServiceProvider.GetService(typeof(IRepository<Request>))!;
                var _workerResolver = (WorkerResolver)scoped.ServiceProvider.GetService(typeof(WorkerResolver))!;

                var request = await _requestsRepository.FirstOrDefaultAsync(new RequestsReadyForProcessing());

                if (request is not null)
                {
                    request.ProcessState = "Begin Processing";
                    request.WorkerInstance = Environment.MachineName;

                    await _requestsRepository.UpdateAsync(request);

                    _logger.LogInformation("{requestId} is processing.", request.Id);

                    await _workerResolver.ProcessAsync(request);

                    _logger.LogInformation("{requestId} is processed.", request.Id);
                }
                //_logger.LogInformation("End scope.");
            }

        }
    }
}
