using Ardalis.GuardClauses;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Core.Lookup;
using EdNexusData.Broker.Core.Services;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Core.Resolvers;

public class BrokerResolver
{
    private readonly ILogger<BrokerResolver> logger;
    private readonly JobStatusService jobStatusService;
    private readonly DirectoryLookupService directoryLookupService;

    public BrokerResolver(
        ILogger<BrokerResolver> logger,
        JobStatusService jobStatusService,
        DirectoryLookupService directoryLookupService
        )
    {
        this.logger = logger;
        this.jobStatusService = jobStatusService;
        this.directoryLookupService = directoryLookupService;
    }

    public async Task<(Uri, string)> Resolve(Job jobInstance, Core.Request request)
    {
        // Determine where to send the information
        jobStatusService.JobRecord = jobInstance;
        await jobStatusService.UpdateJobStatus(JobStatus.Running, "Resolving domain {0}", request.RequestManifest?.To?.District?.Domain);

        Guard.Against.Null(request.RequestManifest?.To?.District?.Domain, "Domain", "Domain is missing");

        var brokerAddress = await directoryLookupService.ResolveBrokerUrl(request.RequestManifest?.To?.District?.Domain!);
        var url = $"https://{brokerAddress.Host}";
        var path = "/" + directoryLookupService.StripPathSlashes(brokerAddress.Path);

        await jobStatusService.UpdateJobStatus(JobStatus.Running, "Resolved domain {0}: url {1} | path {2}", request.RequestManifest?.To?.District?.Domain!, url, path);

        return (new Uri(url), path);
    }
}