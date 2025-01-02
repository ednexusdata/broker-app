using System.ComponentModel;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Core.Resolvers;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Interfaces;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Send Message")]
public class SendMessageJob : IJob
{
    private readonly MessageService messageService;
    private readonly HttpClient httpClient;
    private readonly BrokerResolver brokerResolver;
    private readonly RequestService requestService;
    private readonly INowWrapper nowWrapper;

    public SendMessageJob(
        MessageService messageService,
        IHttpClientFactory httpClientFactory,
        BrokerResolver brokerResolver,
        RequestService requestService,
        INowWrapper nowWrapper
    )
    {
        this.messageService = messageService;
        httpClient = httpClientFactory.CreateClient("default");
        this.brokerResolver = brokerResolver;
        this.requestService = requestService;
        this.nowWrapper = nowWrapper;
    }

    public async Task ProcessAsync(Job jobInstance)
    {
        // Step 1: Resolve request
        var request = await requestService.Get(jobInstance);
        _ = request ?? throw new NullReferenceException($"Request {jobInstance.ReferenceGuid} must resolve.");

        // Step 2: Create message
        var message = await messageService.CreateFromJob(jobInstance, request);

        // Step 3: Prepare to send
        _ = message.MessageContents ?? throw new NullReferenceException($"Message contents missing from message.");
        var formContent = messageService.PrepareJsonContent(message.MessageContents);

        // Step 4: Resolve broker address
        var resolvedBroker = await brokerResolver.Resolve(request, "api/v1/messages", jobInstance);
        _ = resolvedBroker ?? throw new NullReferenceException("Unable to resolve broker for request");

        // Step 5: Send message to broker
        httpClient.BaseAddress = resolvedBroker.HostToUri();
        var result = await httpClient.PostAsync(resolvedBroker.Path, formContent);

        // Step 6: Clean up message
        await messageService.MarkSent(message, result, jobInstance);
    }
}