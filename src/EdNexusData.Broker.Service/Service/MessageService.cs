using System.Text.Json;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Data;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.Domain.Worker;
using EdNexusData.Broker.Service.Worker;
using Microsoft.AspNetCore.Identity;

namespace EdNexusData.Broker.Service;

public class MessageService
{
    private readonly IRepository<Message> _messageRepo;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<Request> _requestRepo;
    private readonly IReadRepository<User> _user;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly JobStatusService<MessageService> _jobStatusService;
    private readonly BrokerDbContext _brokerDbContext;

    public MessageService(IRepository<Message> messageRepo,
                        IRepository<PayloadContent> payloadContentRepository,
                        IRepository<Request> requestRepo,
                        JobStatusService<MessageService> jobStatusService,
                        BrokerDbContext brokerDbContext,
                        IReadRepository<User> user,
                        UserManager<IdentityUser<Guid>> userManager)
    {
        _messageRepo = messageRepo;
        _payloadContentRepository = payloadContentRepository;
        _requestRepo = requestRepo;
        _jobStatusService = jobStatusService;
        _brokerDbContext = brokerDbContext;
        _user = user;
        _userManager = userManager;
    }

    public async Task<Message> Create(Job jobInstance, Request request)
    {
        await _jobStatusService.UpdateRequestStatus(jobInstance, request, null, "Create message and move attachments");

        Guard.Against.Null(request);

        var transaction = _brokerDbContext.Database.BeginTransaction();

        // Create Message
        var message = new Message()
        {
            RequestId = request.Id,
            RequestResponse = RequestResponse.Request,
            MessageContents = new MessageContents()
        };

        await _messageRepo.AddAsync(message);

        if (request.IncomingOutgoing == IncomingOutgoing.Incoming && request.RequestManifest?.Contents is null)
        {
            request.RequestManifest!.Contents = new List<ManifestContent>();
        } else if (request.IncomingOutgoing == IncomingOutgoing.Outgoing && request.ResponseManifest?.Contents is null)
        {
            request.ResponseManifest!.Contents = new List<ManifestContent>();
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

        // Append to contents of manifest
        await _requestRepo.UpdateAsync(request);

        // Move request manifest to message
        if (request.IncomingOutgoing == IncomingOutgoing.Incoming)
        {
            message.MessageContents.Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.RequestManifest));
        } else if (request.IncomingOutgoing == IncomingOutgoing.Outgoing)
        {
            message.MessageContents.Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.ResponseManifest));
        }

        // Set sender
        message.Sender = request.RequestManifest?.From?.Sender;

        await _messageRepo.UpdateAsync(message);
    
        transaction.Commit();
        
        await _jobStatusService.UpdateRequestStatus(jobInstance, request, null, "Finished creating message and moving attachments");

        return message;
    }

    public async Task<MessageTransmission> PrepareTransmission(Message message, Guid fromUserId)
    {
        // Find user
        var user = await _user.GetByIdAsync(fromUserId);
        var userIdentity = await _userManager.FindByIdAsync(fromUserId.ToString());

        Guard.Against.Null(user, "User not found.");
    
        return new MessageTransmission()
        {
            Sender = new EducationOrganizationContact()
            {
                Name = user.Name,
                Email = userIdentity?.Email?.ToLower()
            },
            SentTimestamp = DateTimeOffset.UtcNow,
            Contents = message.MessageContents?.Contents
        };
    }

    public async Task<Message> MarkSent(Message message)
    {
        // Get message
        var latestMessage = await _messageRepo.GetByIdAsync(message.Id);
        
        latestMessage!.MessageTimestamp = DateTime.UtcNow;
        latestMessage!.SenderSentTimestamp = DateTime.UtcNow;
        await _messageRepo.UpdateAsync(latestMessage);

        return latestMessage;
    }
}