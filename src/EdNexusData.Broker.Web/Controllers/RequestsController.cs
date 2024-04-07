using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.ViewModels.Requests;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferOutgoingRecords)]
public class RequestsController : AuthenticatedController<RequestsController>
{
    private readonly IReadRepository<Request> _requestRepository;
    private readonly IReadRepository<Message> _messageRepository;
    private readonly IReadRepository<PayloadContent> _payloadContentRepository;

    public RequestsController(IReadRepository<Request> requestRepository,
        IReadRepository<Message> messageRepository,
        IReadRepository<PayloadContent> payloadContentRepository)
    {
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _payloadContentRepository = payloadContentRepository;
    }

    public async Task<IActionResult> View(Guid id)
    {
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdWithMessagesPayloadContents(id));
        
        Guard.Against.Null(request);

        var payloadContents = request.PayloadContents;
        var releasingFileNames = request.ResponseManifest?.Contents?.Select(x => x.FileName).ToList();
        var releasingPayloadContents = (releasingFileNames?.Count > 0) ? request.PayloadContents?.Where(y => releasingFileNames!.Contains(y.FileName!)).ToList() : null;
        
        var requestingFileNames = request.ResponseManifest?.Contents?.Select(x => x.FileName).ToList();
        var requestingPayloadContents = (requestingFileNames?.Count > 0) ? request.PayloadContents?.Where(y => requestingFileNames!.Contains(y.FileName!)).ToList() : null;

        return View(
            new RequestViewModel() { 
                Request = request, 
                ReleasingPayloadContents = releasingPayloadContents, 
                RequestingPayloadContents = requestingPayloadContents
            }
        );
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ViewMessage(Guid id)
    {
        var message = await _messageRepository.GetByIdAsync(id);

        Guard.Against.Null(message);

        return Ok(message.MessageContents?.ToJsonString());
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ViewAttachment(Guid id)
    {
        var payloadContent = await _payloadContentRepository.GetByIdAsync(id);

        Guard.Against.Null(payloadContent);
        Guard.Against.Null(payloadContent.ContentType, "ContentType", "ContentType missing from payload content.");

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
