using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Ardalis.GuardClauses;
using Azure.Messaging;
using EdNexusData.Broker.Connector;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Internal;
using EdNexusData.Broker.Domain.Worker;
using EdNexusData.Broker.Service.Resolvers;
using EdNexusData.Broker.Service.Services;
using EdNexusData.Broker.SharedKernel;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Service.Jobs;

[Description("Send Message")]
public class SendMessageJob : IJob
{
    private readonly ILogger<SendMessageJob> logger;
    private readonly IRepository<Message> messageRepository;
    private readonly MessageService messageService;
    private readonly HttpClient httpClient;
    private readonly JobStatusService jobStatusService;
    private readonly BrokerResolver brokerResolver;
    private readonly RequestResolver requestResolver;

    public SendMessageJob(
        ILogger<SendMessageJob> logger,
        IRepository<Message> messageRepository,
        MessageService messageService,
        IHttpClientFactory httpClientFactory,
        JobStatusService jobStatusService,
        BrokerResolver brokerResolver,
        RequestResolver requestResolver
    )
    {
        this.logger = logger;
        this.messageRepository = messageRepository;
        this.messageService = messageService;
        this.httpClient = httpClientFactory.CreateClient("default");
        this.jobStatusService = jobStatusService;
        this.brokerResolver = brokerResolver;
        this.requestResolver = requestResolver;
    }

    public async Task ProcessAsync(Job jobInstance)
    {
        var messageContents = JsonSerializer.Deserialize<MessageContents>(jobInstance.JobParameters!);

        var request = await requestResolver.Resolve(jobInstance);

        Guard.Against.Null(request, "request", "Request required");

        var message = await messageService.Create(jobInstance, request);

        // add request message
        var requestMessage = new Message()
        {
            MessageTimestamp = DateTime.UtcNow,
            RequestStatus = messageContents?.RequestStatus,
            RequestResponse = RequestResponse.Request,
            MessageContents = new MessageContents()
            {
                Contents = JsonSerializer.SerializeToDocument(messageContents)
            }
        };
        await messageRepository.AddAsync(requestMessage);

        var formContent = new StringContent(JsonSerializer.Serialize(messageContents), Encoding.UTF8, "application/json");

        var resolvedBroker = await brokerResolver.Resolve(jobInstance, request);
        httpClient.BaseAddress = resolvedBroker.Item1;
        
        var result = await httpClient.PostAsync(resolvedBroker.Item2 + "api/v1/messages", formContent);

        var content = await result.Content.ReadAsStringAsync();
        var jsonReturnedContent = JsonSerializer.Deserialize<MessageContents>(content);

        message.TransmissionDetails = JsonSerializer.SerializeToDocument(FormatTransmissionMessage(result));

        // mark message as sent
        await messageService.MarkSent(message);
    }

    private TransmissionMessage FormatTransmissionMessage(HttpResponseMessage http)
    {
        var requestContent = new TransmissionContent()
        {
            Headers = http.RequestMessage!.Headers.ToDictionary(x => x.Key, y => y.Value)
        };

        var responseContent = new TransmissionContent()
        {
            StatusCode = http.StatusCode,
            Headers = http.Headers.ToDictionary(x => x.Key, y => y.Value)
        };

        return new TransmissionMessage()
        {
            Request = requestContent,
            Response = responseContent
        };
    }
}