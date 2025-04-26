// using EdNexusData.Broker.Core;
// using Microsoft.Extensions.DependencyInjection;
// using Ardalis.GuardClauses;
// using EdNexusData.Broker.Core.Jobs;
// using Microsoft.Extensions.Logging;
// using EdNexusData.Broker.Core.Worker;

// namespace EdNexusData.Broker.Core.Resolvers;

// public class WorkerResolver
// {
//     private readonly IServiceProvider _serviceProvider;
//     private readonly ILogger<WorkerResolver> _logger;

//     public WorkerResolver(IServiceProvider serviceProvider, ILogger<WorkerResolver> logger)
//     {
//         _serviceProvider = serviceProvider;
//         _logger = logger;
//     }

//     public async Task<Job> ProcessAsync(Job jobInstance)
//     {
//         Guard.Against.Null(jobInstance);
        
//         using (var scoped = _serviceProvider.CreateScope())
//         {
//             //_logger.LogInformation("Start worker scope.");
//             // Figure out which job to execute based on the state of the submitted job
//             switch (request.RequestStatus)
//             {
//                 case RequestStatus.WaitingToSend:
//                     var sendRequest = (SendRequest)scoped.ServiceProvider.GetService(typeof(SendRequest))!;
//                     await sendRequest.Process(request);
//                     break;
//                 case RequestStatus.WaitingToExtract:
//                     var payloadJobLoader = (PayloadJobLoader)scoped.ServiceProvider.GetService(typeof(PayloadJobLoader))!;
//                     await payloadJobLoader.Process(request);
//                     break;
//                 case RequestStatus.WaitingToPrepare:
//                     var prepareMappingLoader = (PrepareMapping)scoped.ServiceProvider.GetService(typeof(PrepareMapping))!;
//                     await prepareMappingLoader.Process(request);
//                     break;
//                 case RequestStatus.WaitingToImport:
//                     var importMappingLoader = (ImportMapping)scoped.ServiceProvider.GetService(typeof(ImportMapping))!;
//                     await importMappingLoader.Process(request);
//                     break;
//             }
//             //_logger.LogInformation("End worker scope.");
//         }
        
//         return request;
//     }
// }