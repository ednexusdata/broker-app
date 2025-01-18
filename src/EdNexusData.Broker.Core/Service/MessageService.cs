using System.Net.Http.Json;
using System.Text.Json;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Interfaces;
using EdNexusData.Broker.Core.Messages;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Services;


/// <summary>
/// Service that creates and prepares "request" messages for delivery.
/// 
/// Initial Request (Requestingjob): Create message, add any attachments, move manifest to message contents, add sender
/// Send Message (SendMessage): Create message, set the message contents, add sender
/// 
/// </summary>
public class MessageService
{
    private readonly IRepository<Message> _messageRepo;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<Request> _requestRepo;
    private readonly EducationOrganizationContactService educationOrganizationContactService;
    private readonly INowWrapper nowWrapper;
    private readonly PayloadContentService payloadContentService;
    private readonly JobStatusService<MessageService> _jobStatusService;
    private readonly DbContext _brokerDbContext;

    public MessageService(IRepository<Message> messageRepo,
                        IRepository<PayloadContent> payloadContentRepository,
                        IRepository<Request> requestRepo,
                        JobStatusService<MessageService> jobStatusService,
                        DbContext brokerDbContext,
                        EducationOrganizationContactService educationOrganizationContactService,
                        INowWrapper nowWrapper,
                        PayloadContentService payloadContentService)
    {
        _messageRepo = messageRepo;
        _payloadContentRepository = payloadContentRepository;
        _requestRepo = requestRepo;
        _jobStatusService = jobStatusService;
        _brokerDbContext = brokerDbContext;
        this.educationOrganizationContactService = educationOrganizationContactService;
        this.nowWrapper = nowWrapper;
        this.payloadContentService = payloadContentService;
    }

    public async Task<Message?> Get(Guid id)
    {
        return await _messageRepo.GetByIdAsync(id);
    }

    public async Task<Message> New(Job jobInstance, Request request, Type messageType)
    {
        _ = request ?? throw new ArgumentNullException("Parameter request missing.");

        var message = new Message()
        {
            RequestId = request.Id,
            RequestResponse = RequestResponse.Request,
            RequestStatus = request.RequestStatus,
            MessageType = messageType.FullName,
            MessageContents = new MessageContents(),
            MessageTimestamp = nowWrapper.UtcNow
        };
        
        await _jobStatusService.UpdateJobStatus(jobInstance, JobStatus.Running, "New message returned");

        return message;
    }
    
    public async Task<Message> Create(Job jobInstance, Request request, Type messageType)
    {
        _ = jobInstance ?? throw new ArgumentNullException("Parameter job missing.");
        _ = request ?? throw new ArgumentNullException("Parameter request missing.");
        
        await _jobStatusService.UpdateRequestStatus(jobInstance, request, null, "Create message and move attachments");

        var transaction = _brokerDbContext.Database.BeginTransaction();

        var message = await New(jobInstance, request, messageType);

        await _messageRepo.AddAsync(message);

        await AppendAttachments(message, request);

        // Append to contents of manifest
        await _requestRepo.UpdateAsync(request);

        // Move request manifest to message
        if (request.IncomingOutgoing == IncomingOutgoing.Incoming)
        {
            message.MessageContents!.Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.RequestManifest));
        } else if (request.IncomingOutgoing == IncomingOutgoing.Outgoing)
        {
            message.MessageContents!.Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.ResponseManifest));
        }

        await _messageRepo.UpdateAsync(message);
    
        transaction.Commit();
        
        await _jobStatusService.UpdateRequestStatus(jobInstance, request, null, "Finished creating message and moving attachments");

        return message;
    }

    public async Task<Message> CreateWithMessageContents(
        Request request, 
        MessageContents messageContents, 
        RequestResponse requestResponse, 
        Microsoft.AspNetCore.Http.HttpResponse? httpResponseMessage = null
    )
    {
        var message = new Message()
        {
            RequestId = request.Id,
            MessageTimestamp = nowWrapper.UtcNow,
            SentTimestamp = messageContents?.SenderSentTimestamp,
            RequestStatus = messageContents?.RequestStatus,
            RequestResponse = requestResponse,
            MessageContents = messageContents,
            MessageType = typeof(Manifest).FullName,
            TransmissionDetails = 
                (httpResponseMessage is not null) 
                ? FormatTransmissionMessage(httpResponseMessage)
                : null,
        };
        await _messageRepo.AddAsync(message);

        return message;
    }

    public async Task<Message> CreateFromJob(Job job, Request request, Type messageType)
    {
        var messageContents = JsonSerializer.Deserialize<MessageContents>(job.JobParameters!);
        
        var message = await New(job, request, messageType);
        message.MessageContents = messageContents;
        message.MessageTimestamp = nowWrapper.UtcNow;
        message.SentTimestamp = messageContents?.SenderSentTimestamp;
        message.MessageContents!.RequestId = request.RequestManifest?.RequestId;
        await _messageRepo.AddAsync(message);

        return message;
    }

    public async Task<Message> CreateFromAPIRequest(string request)
    {
        var messageTransmission = JsonSerializer.Deserialize<MessageContents>(request, Defaults.JsonSerializerDefaults.PropertyNameCaseInsensitive);
        _ = messageTransmission ?? throw new NullReferenceException("Missing message");
        _ = messageTransmission.EducationOrganizationId ?? throw new NullReferenceException("Missing sending education organization Id");

        // Verify if request exists
        var requestId = messageTransmission.RequestId;
        if (requestId is not null)
        {
            var existingRequest = await _requestRepo.FirstOrDefaultAsync(new RequestByIdNotEdOrg(requestId.Value, messageTransmission.EducationOrganizationId.Value));
            if (existingRequest is null)
            {
                var inManifest = await _requestRepo.FirstOrDefaultAsync(new RequestByIdInManifest(requestId.Value, messageTransmission.EducationOrganizationId.Value));
                requestId = inManifest?.Id;
            }
        }

        _ = requestId ?? throw new NullReferenceException("Unable to resolve current request ID to send");

        var message = new Message()
        {
            RequestId = requestId!.Value,
            RequestResponse = RequestResponse.Response,
            MessageTimestamp = nowWrapper.UtcNow,
            SentTimestamp = messageTransmission.SenderSentTimestamp,
            MessageContents = messageTransmission,
            RequestStatus = messageTransmission.RequestStatus,
            MessageStatus = MessageStatus.Received,
            MessageType = messageTransmission!.MessageType
        };
        await _messageRepo.AddAsync(message);

        return message;
    }

    public async Task<Message> AppendAttachments(Message message, Request request)
    {
        _ = request ?? throw new ArgumentNullException("Parameter message missing.");
        _ = request ?? throw new ArgumentNullException("Parameter request missing.");
        _ = request.RequestManifest ?? throw new NullReferenceException("RequestManifest cannot be null for request.");

        // Initialize List<ManifestContents> if null
        if (request.RequestManifest is not null && request.RequestManifest.Contents is null && request.IncomingOutgoing == IncomingOutgoing.Incoming)
        {
            request.RequestManifest.Contents = new List<ManifestContent>();
        } else if (request.ResponseManifest is not null && request.ResponseManifest.Contents is null && request.IncomingOutgoing == IncomingOutgoing.Outgoing)
        {
            request.ResponseManifest.Contents = new List<ManifestContent>();
        }
        
        // Move any payloadcontents (attachments) to message
        var attachments = await _payloadContentRepository.ListAsync(new PayloadContentsByRequestId(request.Id));
        if (attachments is not null && attachments.Count > 0)
        {
            foreach(var payloadContent in attachments)
            {
                payloadContent.MessageId = message.Id;
                await _payloadContentRepository.UpdateAsync(payloadContent);
                
                if (request.IncomingOutgoing == IncomingOutgoing.Incoming)
                {
                    request.RequestManifest?.Contents?.Add(new ManifestContent() {
                        Id = payloadContent.Id,
                        ContentType = payloadContent.ContentType!,
                        FileName = payloadContent.FileName!
                    });
                }
                else if (request.IncomingOutgoing == IncomingOutgoing.Outgoing)
                {
                    request.ResponseManifest?.Contents?.Add(new ManifestContent() {
                        Id = payloadContent.Id,
                        ContentType = payloadContent.ContentType!,
                        FileName = payloadContent.FileName!
                    });
                }
            }
        }

        return message;
    }

    public JsonContent PrepareJsonContent(Job job)
    {
        var messageContents = JsonSerializer.Deserialize<MessageContents>(job.JobParameters!);
        _ = messageContents ?? throw new NullReferenceException("Job parameters unable to deseralize to MessageContents");

        return JsonContent.Create(messageContents);
    }

    public JsonContent PrepareJsonContent(MessageContents messageContents)
    {
        return JsonContent.Create(messageContents);
    }

    public async Task<MultipartContent> PrepareMultipartContent(Message message, Request request)
    {
        MultipartFormDataContent multipartContent = new();
        Guid? processUser = null;
        switch (request.IncomingOutgoing)
        {
            case IncomingOutgoing.Incoming:
                processUser = request.RequestProcessUserId!.Value;
            break;
            case IncomingOutgoing.Outgoing:
                processUser = request.ResponseProcessUserId!.Value;
            break;
        }
        _ = processUser ?? throw new NullReferenceException("Request or response process user missing");
        var jsonContent = JsonContent.Create(await PrepareTransmission(message, processUser.Value));
        multipartContent.Add(jsonContent, "manifest");
        // Add on attachments
        multipartContent = await payloadContentService.AddAttachments(multipartContent, message);

        return multipartContent;
    }

    public async Task<MessageContents> PrepareTransmission(Message message, Guid fromUserId)
    {    
        return new MessageContents()
        {
            Sender = await educationOrganizationContactService.FromUser(fromUserId),
            SenderSentTimestamp = nowWrapper.UtcNow,
            Contents = message.MessageContents?.Contents,
            EducationOrganizationId = message.Request?.EducationOrganizationId,
            MessageText = $"Request sent with request id: {message.RequestId}",
            RequestStatus = message.RequestStatus
        };
    }

    public async Task<Message> CreateChatMessage(Guid id, string messageText, Guid fromUserId)
    {
        var request = await _requestRepo.GetByIdAsync(id);
        _ = request ?? throw new NullReferenceException($"Unable to find request {id}");
        
        var message = new Message()
        {
            RequestId = request.Id,
            RequestResponse = RequestResponse.Request,
            RequestStatus = request.RequestStatus,
            MessageType = typeof(ChatMessage).FullName,
            MessageContents = new MessageContents() {
                RequestId = request.RequestManifest?.RequestId,
                MessageText = messageText,
                Sender = await educationOrganizationContactService.FromUser(fromUserId),
                SenderSentTimestamp = nowWrapper.UtcNow,
                RequestStatus = request.RequestStatus,
                EducationOrganizationId = request.EducationOrganizationId,
                Contents = null,
                MessageType = typeof(ChatMessage).FullName
            },
            MessageTimestamp = nowWrapper.UtcNow
        };

        await _messageRepo.AddAsync(message);

        return message;
    }

    public async Task<Message> MarkSent(Message message, HttpResponseMessage httpResponseMessage, RequestStatus? requestStatus, Job? job = null)
    {
        message.SentTimestamp = nowWrapper.UtcNow;
        message.MessageStatus = MessageStatus.Sent;

        if (httpResponseMessage is not null)
        {
            message.TransmissionDetails = FormatTransmissionMessage(httpResponseMessage);
        }
        await _messageRepo.UpdateAsync(message);

        if (job is not null)
        {
            await _jobStatusService.UpdateMessageStatus(job, message, requestStatus, "Message marked as sent");
        }

        return message;
    }

    private TransmissionMessage FormatTransmissionMessage(HttpResponseMessage http)
    {
        var requestContent = new TransmissionContent()
        {
            StringHeaders = http.Headers.ToDictionary(x => x.Key, y => y.Value)
        };

        var responseContent = new TransmissionContent()
        {
            HttpStatusCode = http.StatusCode,
            StringHeaders = http.Headers.ToDictionary(x => x.Key, y => y.Value)
        };

        return new TransmissionMessage()
        {
            Request = requestContent,
            Response = responseContent
        };
    }
    
    private TransmissionMessage FormatTransmissionMessage(Microsoft.AspNetCore.Http.HttpResponse http)
    {
        var requestContent = new TransmissionContent()
        {
            Headers = http.Headers.ToDictionary(x => x.Key, y => y.Value)
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