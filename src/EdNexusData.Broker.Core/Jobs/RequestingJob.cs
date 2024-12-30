using Microsoft.Extensions.Logging;
using EdNexusData.Broker.Core.Specifications;
using DnsClient;
using EdNexusData.Broker.Core.Lookup;
using Ardalis.GuardClauses;
using System.Text.Json;
using System.Net.Http.Json;
using EdNexusData.Broker.Core.Worker;
using System.ComponentModel;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Resolvers;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Requesting Job")]
public class RequestingJob : IJob
{
    private readonly JobStatusService<RequestingJob> jobStatusService;
    private readonly DirectoryLookupService _directoryLookupService;
    private readonly MessageService messageService;
    private readonly RequestService requestService;
    private readonly HttpClient httpClient;

    public RequestingJob(JobStatusService<RequestingJob> jobStatusService,
                        DirectoryLookupService directoryLookupService, 
                        IHttpClientFactory httpClientFactory,
                        MessageService messageService,
                        RequestService requestService)
    {
        this.jobStatusService = jobStatusService;
        _directoryLookupService = directoryLookupService;
        this.messageService = messageService;
        this.requestService = requestService;
        httpClient = httpClientFactory.CreateClient("default");
    }
    
    public async Task ProcessAsync(Job jobInstance)
    {
        // Step 1: Resolve request
        var request = await requestService.Get(jobInstance);
        _ = request ?? throw new NullReferenceException($"Unable to find request id {jobInstance.ReferenceGuid}.");
        
        // Step 2: Create message using request's manifest
        var message = await messageService.Create(jobInstance, request);
        var messageContent = JsonSerializer.Deserialize<Manifest>(message.MessageContents?.Contents!);
        _ = messageContent ?? throw new InvalidCastException("Message contents did not deseralize to manifest succesfully.");

        // Step 3: Resolve broker address
        _ = messageContent?.To?.District?.Domain ?? throw new NullReferenceException("Domain is missing");
        await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Requesting, 
            "Resolving domain {0}", messageContent.To.District.Domain);
        var brokerAddress = await _directoryLookupService.ComposeBrokerUrl(messageContent.To.District.Domain, "api/v1/requests");
        await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Requesting, 
            "Resolved domain with path {0}: url {1} | path {2}", messageContent.To.District.Domain, brokerAddress?.Host, brokerAddress?.Path);

        // Step 4: Prepare POST request
        var multipartContent = await messageService.PrepareMultipartContent(message, request);

        // Step 5: Send Request and get result
        httpClient.BaseAddress = brokerAddress?.HostToUri();
        var result = await httpClient.PostAsync(brokerAddress?.Path, multipartContent);
        var content = await result.Content.ReadFromJsonAsync<MessageContents>(Defaults.JsonSerializerDefaults.PropertyNameCaseInsensitive);
        await jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Requesting, 
            "Sent request result: {0} / {1}", result.StatusCode, content?.Contents.ToJsonString());
        multipartContent.Dispose();

        // Step 6: Update message as sent with http transmission info
        await messageService.MarkSent(message, result, jobInstance);

        // Step 7: Create message with response
        if (content is not null)
        {
            await messageService.CreateWithMessageContents(request, content);
        }

        // Step 8: Update request to sent
        await requestService.MarkRequested(request, jobInstance);
    }
}