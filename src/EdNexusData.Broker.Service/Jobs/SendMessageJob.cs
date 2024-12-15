using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Ardalis.GuardClauses;
using Azure.Messaging;
using EdNexusData.Broker.Connector;
using EdNexusData.Broker.Domain;
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
        HttpClient httpClient,
        JobStatusService jobStatusService,
        BrokerResolver brokerResolver,
        RequestResolver requestResolver
    )
    {
        this.logger = logger;
        this.messageRepository = messageRepository;
        this.messageService = messageService;
        this.httpClient = httpClient;
        this.jobStatusService = jobStatusService;
        this.brokerResolver = brokerResolver;
        this.requestResolver = requestResolver;
    }

    public async Task ProcessAsync(Job jobInstance)
    {
        var messageContents = JsonSerializer.Deserialize<MessageContent>(jobInstance.JobParameters!);

        var request = await requestResolver.Resolve(jobInstance);

        Guard.Against.Null(request, "request", "Request required");

        var formContent = new StringContent(JsonSerializer.Serialize(messageContents), Encoding.UTF8, "application/json");

        var resolvedBroker = await brokerResolver.Resolve(jobInstance, request);
        httpClient.BaseAddress = resolvedBroker.Item1;
        
        var result = await httpClient.PostAsync(resolvedBroker + "api/v1/requests", formContent);

        var content = await result.Content.ReadAsStringAsync();
    }
}