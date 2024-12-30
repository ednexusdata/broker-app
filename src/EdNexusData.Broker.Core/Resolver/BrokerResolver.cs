using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Core.Lookup;
using EdNexusData.Broker.Core.Models;

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

    public async Task<BrokerUrl?> Resolve(Request request, string? path = null, Job? jobInstance = null)
    {
        await jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Resolving domain {0}", request.RequestManifest?.To?.District?.Domain);

        _ = request.RequestManifest?.To?.District?.Domain ?? throw new NullReferenceException($"Domain is missing from requestmanifest for request {request.Id}");

        var brokerAddress = await ComposeBrokerUrl(request.RequestManifest?.To?.District?.Domain!, path);
        if (brokerAddress is not null)
        {
            await jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "Resolved domain {0}: url {1} | path {2}", request.RequestManifest?.To?.District?.Domain!, brokerAddress.Host, brokerAddress.Path);
        }
        
        return brokerAddress;
    }

    public async Task<BrokerUrl?> ComposeBrokerUrl(string searchDomain, string? path = null)
    {
        var brokerTxtRecord = await directoryLookupService.ResolveBroker(searchDomain);

        if (brokerTxtRecord is not null && brokerTxtRecord.Host is not null)
        {
            var brokerUrl = new BrokerUrl()
            {
                Host = brokerTxtRecord.Host,
                Path = (path is not null) ? "/" + directoryLookupService.StripPathSlashes(brokerTxtRecord.Path) + path : null
            };
            return brokerUrl;
        }
        return null;
    }
}