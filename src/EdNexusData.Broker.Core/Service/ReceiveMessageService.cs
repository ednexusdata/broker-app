using System.Net;
using System.Text.Json;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Services;

public class ReceiveMessageService
{
    private readonly IReadRepository<EducationOrganization> educationOrganizationRepository;
    private readonly RequestService requestService;
    private readonly MessageService messageService;
    private readonly PayloadContentService payloadContentService;

    public ReceiveMessageService(
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        RequestService requestService,
        MessageService messageService,
        PayloadContentService payloadContentService
    )
    {
        this.educationOrganizationRepository = educationOrganizationRepository;
        this.requestService = requestService;
        this.messageService = messageService;
        this.payloadContentService = payloadContentService;
    }

    public async Task<MessageContents> ReceiveRequest(string webRequest, List<Models.File>? files = null, Microsoft.AspNetCore.Http.HttpResponse? httpResponseMessage = null)
    {
        var messageTransmission = JsonSerializer.Deserialize<MessageContents>(webRequest, Defaults.JsonSerializerDefaults.PropertyNameCaseInsensitive);
        _ = messageTransmission ?? throw new NullReferenceException("Unable to deseralize MessageContents");
            
        var mainfestJson = JsonSerializer.Deserialize<Manifest>(messageTransmission.Contents!, Defaults.JsonSerializerDefaults.PropertyNameCaseInsensitive);
        _ = mainfestJson ?? throw new NullReferenceException("Unable to deseralize Manifest");

        var educationOrganizationId = mainfestJson?.To?.School?.Id;
        _ = educationOrganizationId ?? throw new NullReferenceException("Missing education organization");

        var toEdOrg = await educationOrganizationRepository.GetByIdAsync(educationOrganizationId.Value);
        _ = toEdOrg ?? throw new NullReferenceException($"EducationOrganization {educationOrganizationId} not found.");

        Request? request = null;
        Message? message = null;

        // Check if request id exists
        if (mainfestJson?.RequestId is not null)
        {
            request = await requestService.Get(mainfestJson.RequestId.Value);

            if (request is not null && request.EducationOrganizationId == educationOrganizationId)
            {
                await requestService.UpdateResponseManifest(request.Id, mainfestJson);
                message = await messageService.CreateWithMessageContents(request, new MessageContents()
                {
                    RequestStatus = messageTransmission.RequestStatus,
                    Sender = messageTransmission.Sender,
                    SenderSentTimestamp = messageTransmission.SenderSentTimestamp,
                    Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.ResponseManifest)),
                    MessageText = messageTransmission.MessageText
                }, RequestResponse.Response, httpResponseMessage);
            }
            else
            {
                // Create request
                request = await requestService.Create(educationOrganizationId.Value, mainfestJson!, RequestStatus.Received, IncomingOutgoing.Outgoing);

                // Create message
                message = await messageService.CreateWithMessageContents(request, new MessageContents()
                {
                    RequestStatus =  RequestStatus.Requested, // messageTransmission.RequestStatus,
                    Sender = messageTransmission.Sender,
                    SenderSentTimestamp = messageTransmission.SenderSentTimestamp,
                    Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.ResponseManifest)),
                    MessageText = messageTransmission.MessageText
                }, RequestResponse.Response, httpResponseMessage);
            }
        }
        else
        {
            // Create request
            request = await requestService.Create(educationOrganizationId.Value, mainfestJson!, RequestStatus.Received, IncomingOutgoing.Outgoing);

            // Create message
            message = await messageService.CreateWithMessageContents(request, new MessageContents()
            {
                RequestStatus =  RequestStatus.Requested, // messageTransmission.RequestStatus,
                Sender = messageTransmission.Sender,
                SenderSentTimestamp = messageTransmission.SenderSentTimestamp,
                Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.ResponseManifest)),
                MessageText = messageTransmission.MessageText
            }, RequestResponse.Response, httpResponseMessage);
        }

        // Process Files
        if (files is not null && files.Count > 0)
        {
            _ = payloadContentService.ProcessFiles(message, files);
        }

        // create message response and return it
        var returnMessageContent = new MessageContents()
        {
            SenderSentTimestamp = DateTimeOffset.UtcNow,
            Sender = new EducationOrganizationContact()
            {
                Name = "Broker " +  Dns.GetHostName()
            },   
            MessageText = string.Format("Request received with id: {0}", request!.Id.ToString()),
            Contents = JsonSerializer.SerializeToDocument(request!.Id.ToString()),
            RequestStatus = RequestStatus.Received,
            RequestId = request!.Id
        };

        await messageService.CreateWithMessageContents(request, returnMessageContent, RequestResponse.Request);

        return returnMessageContent;
    }

    public async Task<MessageContents> ReceiveTransmit(string webRequest, List<Models.File>? files = null, Microsoft.AspNetCore.Http.HttpResponse? httpResponseMessage = null)
    {
        var messageTransmission = JsonSerializer.Deserialize<MessageContents>(webRequest, Defaults.JsonSerializerDefaults.PropertyNameCaseInsensitive);
        _ = messageTransmission ?? throw new NullReferenceException("Unable to deseralize MessageContents");
            
        var mainfestJson = JsonSerializer.Deserialize<Manifest>(messageTransmission.Contents!, Defaults.JsonSerializerDefaults.PropertyNameCaseInsensitive);
        _ = mainfestJson ?? throw new NullReferenceException("Unable to deseralize Manifest");

        var educationOrganizationId = mainfestJson?.To?.School?.Id;
        _ = educationOrganizationId ?? throw new NullReferenceException("Missing education organization");

        var toEdOrg = await educationOrganizationRepository.GetByIdAsync(educationOrganizationId.Value);
        _ = toEdOrg ?? throw new NullReferenceException($"EducationOrganization {educationOrganizationId} not found.");

        _ = mainfestJson!.RequestId ?? throw new NullReferenceException("RequestId missing from manifest.");
        var request = await requestService.Get(mainfestJson.RequestId.Value);
        _ = request ?? throw new NullReferenceException("Request not found");

        await requestService.UpdateResponseManifest(request.Id, mainfestJson);
        var message = await messageService.CreateWithMessageContents(request, new MessageContents()
        {
            RequestStatus = RequestStatus.Transmitted, // messageTransmission.RequestStatus,
            Sender = messageTransmission.Sender,
            SenderSentTimestamp = messageTransmission.SenderSentTimestamp,
            Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.ResponseManifest)),
            MessageText = messageTransmission.MessageText
        }, RequestResponse.Response, httpResponseMessage);

        // Process Files
        if (files is not null && files.Count > 0)
        {
            _ = await payloadContentService.ProcessFiles(message, files);
        }

        // create message response and return it
        var returnMessageContent = new MessageContents()
        {
            SenderSentTimestamp = DateTimeOffset.UtcNow,
            Sender = new EducationOrganizationContact()
            {
                Name = "Broker " +  Dns.GetHostName()
            },   
            MessageText = string.Format("Request received with id: {0}", request!.Id.ToString()),
            Contents = JsonSerializer.SerializeToDocument(request!.Id.ToString()),
            RequestStatus = RequestStatus.Transmitted,
            RequestId = request!.Id
        };

        await messageService.CreateWithMessageContents(request, returnMessageContent, RequestResponse.Request);
        await requestService.UpdateStatus(request, RequestStatus.Transmitted);

        return returnMessageContent;
    }
}