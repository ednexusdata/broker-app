using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.ViewModels.Requests;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Core.Jobs;
using EdNexusData.Broker.Core.Services;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferOutgoingRecords)]
public class RequestsController : AuthenticatedController<RequestsController>
{
    private readonly IReadRepository<Request> _requestRepository;
    private readonly IReadRepository<Message> _messageRepository;
    private readonly IReadRepository<PayloadContent> _payloadContentRepository;
    private readonly JobService jobService;
    private readonly ICurrentUser currentUser;
    private readonly CurrentUserHelper currentUserHelper;
    private readonly MessageService messageService;
    private readonly RequestService requestService;

    public RequestsController(IReadRepository<Request> requestRepository,
        IReadRepository<Message> messageRepository,
        IReadRepository<PayloadContent> payloadContentRepository,
        JobService jobService,
        ICurrentUser currentUser,
        CurrentUserHelper currentUserHelper,
        MessageService messageService,
        RequestService requestService)
    {
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _payloadContentRepository = payloadContentRepository;
        this.jobService = jobService;
        this.currentUser = currentUser;
        this.currentUserHelper = currentUserHelper;
        this.messageService = messageService;
        this.requestService = requestService;
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

        var payloadContents = request.PayloadContents;
        var releasingFileNames = request.ResponseManifest?.Contents?.Select(x => x.FileName).ToList();
        var releasingPayloadContents = (releasingFileNames?.Count > 0) ? request.PayloadContents?.Where(y => releasingFileNames!.Contains(y.FileName!)).ToList() : null;
        
        var requestingFileNames = request.ResponseManifest?.Contents?.Select(x => x.FileName).ToList();
        var requestingPayloadContents = (requestingFileNames?.Count > 0) ? request.PayloadContents?.Where(y => requestingFileNames!.Contains(y.FileName!)).ToList() : null;

        var requestViewModel = new RequestViewModel() { 
            Request = request, 
            ReleasingPayloadContents = releasingPayloadContents, 
            RequestingPayloadContents = requestingPayloadContents,
            DisplayMessagesType = RequestViewModel.DisplayMessageType.ChatMessages
        };

        requestViewModel.SetStatusGrid(currentUserHelper);

        return View(requestViewModel);
    }

    public async Task<IActionResult> ViewWithTransmissions(Guid id, Guid? jobId)
    {
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdWithMessagesPayloadContents(id));
        Guard.Against.Null(request);

        var messages = await _messageRepository.ListAsync(new MessagesForRequest(request.Id));

        if (request.RequestStatus.NotIn(RequestStatus.Requested, RequestStatus.Transmitted, RequestStatus.Received))
        {
            ViewBag.JobId = jobId;
        }

        var payloadContents = request.PayloadContents;
        var releasingFileNames = request.ResponseManifest?.Contents?.Select(x => x.FileName).ToList();
        var releasingPayloadContents = (releasingFileNames?.Count > 0) ? request.PayloadContents?.Where(y => releasingFileNames!.Contains(y.FileName!)).ToList() : null;
        
        var requestingFileNames = request.ResponseManifest?.Contents?.Select(x => x.FileName).ToList();
        var requestingPayloadContents = (requestingFileNames?.Count > 0) ? request.PayloadContents?.Where(y => requestingFileNames!.Contains(y.FileName!)).ToList() : null;

        var requestViewModel = new RequestViewModel() { 
            Request = request, 
            ReleasingPayloadContents = releasingPayloadContents, 
            RequestingPayloadContents = requestingPayloadContents,
            DisplayMessagesType = RequestViewModel.DisplayMessageType.TransmissionMessages
        };

        requestViewModel.SetStatusGrid(currentUserHelper);

        return View("View", requestViewModel);
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

        return Ok(message.TransmissionDetails?.ToJsonDocument().ToJsonString());
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

    [HttpPost]
    public async Task<IActionResult> SendMessage(Guid id, string messageText)
    {
        var message = await messageService.CreateChatMessage(id, messageText, currentUser.AuthenticatedUserId()!.Value);
        
        var job = await jobService.CreateJobAsync(typeof(SendMessageJob), typeof(Message), message.Id, currentUser.AuthenticatedUserId());
        
        TempData[VoiceTone.Positive] = $"Message submitted to send.";
        return RedirectToAction(nameof(View), "Requests", new { id = id, jobId = job.Id });
    }

    [HttpPut]
    public async Task<IActionResult> Open(Guid id)
    {
        var request = await requestService.Open(id);

        TempData[VoiceTone.Positive] = $"Request opened.";
        return RedirectToAction("Index", "Preparing", new { id = id });
    }

    [HttpPut]
    public async Task<IActionResult> Close(Guid id)
    {
        var request = await requestService.Close(id);

        TempData[VoiceTone.Positive] = $"Request closed.";
        return RedirectToAction(nameof(View), "Requests", new { id = id });
    }
}
