using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.ViewModels.Requests;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferOutgoingRecords)]
public class RequestsController : AuthenticatedController<RequestsController>
{
    private readonly IReadRepository<Request> _requestRepository;
    private readonly IReadRepository<Message> _messageRepository;
    private readonly IReadRepository<PayloadContent> _payloadContentRepository;
    private readonly CurrentUserHelper currentUserHelper;

    public RequestsController(IReadRepository<Request> requestRepository,
        IReadRepository<Message> messageRepository,
        IReadRepository<PayloadContent> payloadContentRepository,
        CurrentUserHelper currentUserHelper)
    {
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _payloadContentRepository = payloadContentRepository;
        this.currentUserHelper = currentUserHelper;
    }

    public async Task<IActionResult> View(Guid id, Guid? jobId)
    {
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdWithMessagesPayloadContents(id));
        Guard.Against.Null(request);

        var messages = await _messageRepository.ListAsync(new MessagesForRequest(request.Id));

        if (request.RequestStatus.NotIn(RequestStatus.Requested, RequestStatus.Transmitted, RequestStatus.Received))
        {
            ViewBag.JobId = jobId;
        }

        var statusGrid = new Dictionary<RequestStatus, StatusGridViewModel>();
        foreach(var message in messages)
        {
            // var messageType = message.MessageContents?.RootElement.GetProperty("MessageType").GetString();
            // var deseralizedMessageContent = JsonConvert.DeserializeObject(message.MessageContents.ToJsonString()!, Type.GetType(messageType!)!);
            if (message.RequestStatus is not null && !statusGrid.ContainsKey(message.RequestStatus.Value))
            {
                if (message.SenderSentTimestamp is not null)
                {
                    statusGrid[message.RequestStatus.Value] = new StatusGridViewModel()
                    {
                        Timestamp = TimeZoneInfo.ConvertTimeFromUtc(message.SenderSentTimestamp!.Value.DateTime, currentUserHelper.ResolvedCurrentUserTimeZone()).ToString("M/dd/yyyy h:mm tt"),
                        Userstamp = message.Sender?.Name
                    };
                }
            }
        }

        var payloadContents = request.PayloadContents;
        var releasingFileNames = request.ResponseManifest?.Contents?.Select(x => x.FileName).ToList();
        var releasingPayloadContents = (releasingFileNames?.Count > 0) ? request.PayloadContents?.Where(y => releasingFileNames!.Contains(y.FileName!)).ToList() : null;
        
        var requestingFileNames = request.ResponseManifest?.Contents?.Select(x => x.FileName).ToList();
        var requestingPayloadContents = (requestingFileNames?.Count > 0) ? request.PayloadContents?.Where(y => requestingFileNames!.Contains(y.FileName!)).ToList() : null;

        return View(
            new RequestViewModel() { 
                Request = request, 
                ReleasingPayloadContents = releasingPayloadContents, 
                RequestingPayloadContents = requestingPayloadContents,
                StatusGrid = statusGrid
            }
        );
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ViewMessage(Guid id)
    {
        var message = await _messageRepository.GetByIdAsync(id);

        Guard.Against.Null(message);

        return Ok(message.MessageContents?.Contents?.ToJsonString());
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ViewTransmission(Guid id)
    {
        var message = await _messageRepository.GetByIdAsync(id);

        Guard.Against.Null(message);

        return Ok(message.TransmissionDetails?.ToJsonString());
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ViewAttachment(Guid id, bool attachmentContentDisposition = false)
    {
        var payloadContent = await _payloadContentRepository.GetByIdAsync(id);

        Guard.Against.Null(payloadContent);
        Guard.Against.Null(payloadContent.ContentType, "ContentType", "ContentType missing from payload content.");

        if (attachmentContentDisposition == true)
        {
            Response.Headers.Append("Content-Disposition", $"attachment;filename={payloadContent.FileName}");
            //return File(fileStream, contentType);
        }
        else
        {
            Response.Headers.Append("Content-Disposition", "inline");
        }
        
        if (payloadContent.JsonContent is not null)
        {
            return Ok(payloadContent.JsonContent.ToJsonString());
        }

        var stream = new MemoryStream();
        if (payloadContent.XmlContent is not null)
        {
            payloadContent.XmlContent.Save(stream);
        }
        if (payloadContent.BlobContent is not null)
        {
            await stream.WriteAsync(payloadContent.BlobContent);
            stream.Seek(0, SeekOrigin.Begin);
        }

        return new FileStreamResult(stream, payloadContent.ContentType);
    }
}
