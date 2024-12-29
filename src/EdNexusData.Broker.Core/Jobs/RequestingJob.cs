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

namespace EdNexusData.Broker.Core.Jobs;

[Description("Requesting Job")]
public class RequestingJob : IJob
{
    private readonly ILogger<RequestingJob> _logger;
    private readonly DbContext _brokerDbContext;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly ILookupClient _lookupClient;
    private readonly JobStatusService<RequestingJob> _jobStatusService;
    private readonly DirectoryLookupService _directoryLookupService;
    private readonly MessageService _messageService;
    private readonly HttpClient _httpClient;

    public RequestingJob( ILogger<RequestingJob> logger, 
                        DbContext brokerDbContext,
                        IRepository<Request> requestRepository, 
                        IRepository<Message> messageRepository,
                        IRepository<Core.PayloadContent> payloadContentRepository,
                        ILookupClient lookupClient,
                        JobStatusService<RequestingJob> jobStatusService,
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
        _httpClient = httpClientFactory.CreateClient("default");
    }
    
    public async Task ProcessAsync(Job jobInstance)
    {
        Guard.Against.Null(jobInstance.ReferenceGuid, "referenceGuid", $"Unable to find request Id {jobInstance.ReferenceGuid}");
        
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(jobInstance.ReferenceGuid.Value));
        
        Guard.Against.Null(request, "request", $"Unable to find request id {jobInstance.ReferenceGuid}");
        
        var message = await _messageService.Create(jobInstance, request);
        var messageContent = JsonSerializer.Deserialize<Manifest>(message.MessageContents?.Contents!);
        var messageToTransmit = await _messageService.PrepareTransmission(message, request.RequestProcessUserId!.Value);

        Guard.Against.Null(messageContent, "Message did not convert to type Manifest");
        Guard.Against.Null(messageContent?.To?.District?.Domain, "Domain is missing");

        // Determine where to send the information
        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Requesting, "Resolving domain {0}", messageContent.To.District.Domain);
        var brokerAddress = await _directoryLookupService.ResolveBrokerUrl(messageContent.To.District.Domain);
        var url = $"https://{brokerAddress.Host}";
        var path = "/" + _directoryLookupService.StripPathSlashes(brokerAddress.Path);

        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Requesting, "Resolved domain {0}: url {1} | path {2}", messageContent.To.District.Domain, url, path);

        // Prepare request
        using MultipartFormDataContent multipartContent = new();
        var jsonContent = JsonContent.Create(messageToTransmit);
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
        var jsonReturnedContent = JsonSerializer.Deserialize<MessageContents>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Requesting, "Sent request result: {0} / {1}", result.StatusCode, content);

        // Update message with http transmission info
        message.TransmissionDetails = JsonSerializer.SerializeToDocument(FormatTransmissionMessage(result));

        // mark message as sent
        await _messageService.MarkSent(message);
        await _jobStatusService.UpdateMessageStatus(jobInstance, message, RequestStatus.Requested, "Message marked as requested.");

        // add response as message
        var responseMessage = new Message()
        {
            Request = request,
            MessageTimestamp = DateTime.UtcNow,
            Sender = jsonReturnedContent?.Sender,
            SenderSentTimestamp = jsonReturnedContent?.SenderSentTimestamp,
            RequestStatus = jsonReturnedContent?.RequestStatus,
            RequestResponse = RequestResponse.Response,
            MessageContents = jsonReturnedContent
        };
        await _messageRepository.AddAsync(responseMessage);

        // Update request to sent
        var dbRequest = await _requestRepository.GetByIdAsync(request.Id);
        dbRequest!.InitialRequestSentDate = DateTime.UtcNow;
        await _requestRepository.UpdateAsync(dbRequest);

        await _jobStatusService.UpdateRequestStatus(jobInstance, request, RequestStatus.Requested, "Finished updating request.");
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