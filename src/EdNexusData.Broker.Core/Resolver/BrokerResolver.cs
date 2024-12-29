using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Core.Lookup;

namespace EdNexusData.Broker.Core.Resolvers;

public class BrokerResolver
{
    private readonly JobStatusService<BrokerResolver> jobStatusService;
    private readonly DirectoryLookupService directoryLookupService;

    public BrokerResolver(
        JobStatusService<BrokerResolver> jobStatusService,
        DirectoryLookupService directoryLookupService
    )
    {
        this.jobStatusService = jobStatusService;
        this.directoryLookupService = directoryLookupService;
    }

    public async Task<(Uri, string)> Resolve(Job jobInstance, Request request)
    {
        await jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Resolving domain {0}", request.RequestManifest?.To?.District?.Domain);

        _ = request.RequestManifest?.To?.District?.Domain ?? throw new NullReferenceException($"Domain is missing from requestmanifest for request {request.Id}");

        var brokerAddress = await directoryLookupService.ResolveBrokerUrl(request.RequestManifest?.To?.District?.Domain!);
        var url = $"https://{brokerAddress.Host}";
        var path = "/" + directoryLookupService.StripPathSlashes(brokerAddress.Path);

        await jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Resolved domain {0}: url {1} | path {2}", request.RequestManifest?.To?.District?.Domain!, url, path);

        return (new Uri(url), path);
    }
}