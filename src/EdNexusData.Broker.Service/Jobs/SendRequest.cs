using System.Net.Mail;
using Microsoft.Extensions.Logging;
using EdNexusData.Broker.Data;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using MimeKit;
using DnsClient;
using EdNexusData.Broker.Service.Lookup;
using Ardalis.GuardClauses;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using EdNexusData.Broker.Service.Worker;

namespace EdNexusData.Broker.Service.Jobs;

public class SendRequest
{
    private readonly ILogger<SendRequest> _logger;
    private readonly BrokerDbContext _brokerDbContext;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly ILookupClient _lookupClient;
    private readonly JobStatusService<SendRequest> _jobStatusService;
    private readonly DirectoryLookupService _directoryLookupService;
    private readonly MessageService _messageService;
    private readonly HttpClient _httpClient;

    public SendRequest( ILogger<SendRequest> logger, 
                        BrokerDbContext brokerDbContext,
                        IRepository<Request> requestRepository, 
                        IRepository<Message> messageRepository,
                        IRepository<PayloadContent> payloadContentRepository,
                        ILookupClient lookupClient,
                        JobStatusService<SendRequest> jobStatusService,
                        DirectoryLookupService directoryLookupService, 
                        IHttpClientFactory httpClientFactory,
                        MessageService messageService)
    {
        _logger = logger;
        _brokerDbContext = brokerDbContext;
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _payloadContentRepository = payloadContentRepository;
        _lookupClient = lookupClient;
        _jobStatusService = jobStatusService;
        _directoryLookupService = directoryLookupService;
        _messageService = messageService;
        _httpClient = httpClientFactory.CreateClient("IgnoreSSL");
    }
    
    public async Task Process(Request request)
    {
        var message = await _messageService.Create(request);
        var messageContent = JsonSerializer.Deserialize<Manifest>(message.MessageContents.ToJsonString()!);

        Guard.Against.Null(messageContent, "Message did not convert to type Manifest");
        Guard.Against.Null(messageContent?.To?.District?.Domain, "Domain is missing");

        // Determine where to send the information
        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Sending, "Resolving domain {0}", messageContent.To.District.Domain);
        var brokerAddress = await _directoryLookupService.ResolveBrokerUrl(messageContent.To.District.Domain);
        var url = $"https://{brokerAddress.Host}";
        var path = "/" + _directoryLookupService.StripPathSlashes(brokerAddress.Path);

        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Sending, "Resolved domain {0}: url {1} | path {2}", messageContent.To.District.Domain, url, path);

        // Prepare request
        using MultipartFormDataContent multipartContent = new();
        var jsonContent = JsonContent.Create(messageContent);
        multipartContent.Add(jsonContent, "manifest");

        // Add on attachments
        var attachments = await _payloadContentRepository.ListAsync(new PayloadContentsByMessageId(message.Id));
        if (attachments is not null && attachments.Count > 0)
        {
            foreach(var attachment in attachments)
            {
                if (attachment.BlobContent != null)
                {
                    multipartContent.Add(new ByteArrayContent(attachment.BlobContent!), "files", attachment.FileName!);
                }
                if (attachment.JsonContent != null)
                {
                    multipartContent.Add(new StringContent(JsonSerializer.Serialize(attachment.JsonContent)), "files", attachment.FileName!);
                }
            }
        }

        // Send Request
        _httpClient.BaseAddress = new Uri(url);
        var result = await _httpClient.PostAsync(path + "api/v1/requests", multipartContent);

        var content = await result.Content.ReadAsStringAsync();

        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Sending, "Sent request result: {0} / {1}", result.StatusCode, content);

        // mark message as sent
        await _messageService.MarkSent(message);

        // Update request to sent
        var dbRequest = await _requestRepository.GetByIdAsync(request.Id);
        dbRequest!.InitialRequestSentDate = DateTime.UtcNow;
        await _requestRepository.UpdateAsync(dbRequest);

        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Sent, "Finished updating request.");
    }
}