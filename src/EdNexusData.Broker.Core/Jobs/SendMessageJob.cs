using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Core.Resolvers;
using EdNexusData.Broker.Core.Services;
using Microsoft.Extensions.Logging;
using EdNexusData.Broker.Core.Interfaces;

namespace EdNexusData.Broker.Core.Jobs;

[Description("Send Message")]
public class SendMessageJob : IJob
{
    private readonly ILogger<SendMessageJob> logger;
    private readonly IRepository<Message> messageRepository;
    private readonly MessageService messageService;
    private readonly HttpClient httpClient;
    private readonly JobStatusService<SendMessageJob> jobStatusService;
    private readonly BrokerResolver brokerResolver;
    private readonly RequestResolver requestResolver;
    private readonly INowWrapper nowWrapper;

    public SendMessageJob(
        ILogger<SendMessageJob> logger,
        IRepository<Message> messageRepository,
        MessageService messageService,
        IHttpClientFactory httpClientFactory,
        JobStatusService<SendMessageJob> jobStatusService,
        BrokerResolver brokerResolver,
        RequestResolver requestResolver,
        INowWrapper nowWrapper
    )
    {
        this.logger = logger;
        this.messageRepository = messageRepository;
        this.messageService = messageService;
        this.httpClient = httpClientFactory.CreateClient("default");
        this.jobStatusService = jobStatusService;
        this.brokerResolver = brokerResolver;
        this.requestResolver = requestResolver;
        this.nowWrapper = nowWrapper;
    }

    public async Task ProcessAsync(Job jobInstance)
    {
        // Step 1: Get message contents
        var messageContents = JsonSerializer.Deserialize<MessageContents>(jobInstance.JobParameters!);

        // Step 2: Resolve request
        var request = await requestResolver.Resolve(jobInstance);
        _ = request ?? throw new NullReferenceException($"Request {jobInstance.ReferenceGuid} must resolve.");

        // Step 3: Create message
        var message = await messageService.New(jobInstance, request);
        message.MessageTimestamp = nowWrapper.UtcNow;
        message.MessageContents = messageContents;
        message.Sender = message.MessageContents?.Sender;
        await messageRepository.AddAsync(message);

        // Step 4: Prepare to send
        var formContent = new StringContent(JsonSerializer.Serialize(messageContents), Encoding.UTF8, "application/json");

        // Step 5: Resolve broker address
        var resolvedBroker = await brokerResolver.Resolve(jobInstance, request);
        httpClient.BaseAddress = resolvedBroker.Item1;
        
        // Step 6: Send message to broker
        var result = await httpClient.PostAsync(resolvedBroker.Item2 + "api/v1/messages", formContent);

        // Step 7: Clean up message
        await messageService.MarkSent(message, await FormatTransmissionMessage(result));
    }

    private async Task<TransmissionMessage> FormatTransmissionMessage(HttpResponseMessage http)
    {
        var requestContent = new TransmissionContent()
        {
            Headers = http.RequestMessage!.Headers.ToDictionary(x => x.Key, y => y.Value)
        };

        var responseContent = new TransmissionContent()
        {
            StatusCode = http.StatusCode,
            Headers = http.Headers.ToDictionary(x => x.Key, y => y.Value),
            Content = await http.Content.ReadAsStringAsync()
        };

        return new TransmissionMessage()
        {
            Request = requestContent,
            Response = responseContent
        };
    }
    
}