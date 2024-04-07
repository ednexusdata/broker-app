using System.Net;
using System.Text.Json;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Controllers;
using EdNexusData.Broker.Web.Utilities;

namespace EdNexusData.Broker.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Route("api/v1/requests")]
public class RequestsController : Controller
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IReadRepository<EducationOrganization> _edOrgRepo;

    public RequestsController(IRepository<Request> requestRepository, 
        IRepository<Message> messageRepository, 
        IRepository<PayloadContent> payloadContentRepository,
        IReadRepository<EducationOrganization> edOrgRepo)
    {
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _payloadContentRepository = payloadContentRepository;
        _edOrgRepo = edOrgRepo;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Receive([FromForm] string manifest, [FromForm] IList<IFormFile> files)
    {
        try
        {
            var mainfestJson = JsonSerializer.Deserialize<Manifest>(manifest, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true 
            });

            // Check if EdOrg exists
            var educationOrganizationId = mainfestJson?.To?.School?.Id;

            Guard.Against.Null(educationOrganizationId);

            var toEdOrg = await _edOrgRepo.GetByIdAsync(educationOrganizationId.Value);
            
            if (toEdOrg is null)
            {
                return NotFound($"EducationOrganization {educationOrganizationId} not found.");
            }

            Request? request = null;
            Message? message = null;

            RequestStatus? statusToSet = null;

            // Check if request id exists
            if (mainfestJson?.RequestId is not null)
            {
                request = await _requestRepository.GetByIdAsync(mainfestJson.RequestId.Value);

                if (request is not null && request.EducationOrganizationId == educationOrganizationId)
                {
                    request.ResponseManifest = mainfestJson;
                    await _requestRepository.UpdateAsync(request);

                    // Create message
                    message = new Message()
                    {
                        RequestId = request.Id,
                        RequestResponse = RequestResponse.Response,
                        MessageTimestamp = DateTime.UtcNow,
                        MessageContents = JsonDocument.Parse(JsonSerializer.Serialize(request.ResponseManifest))
                    };
                    await _messageRepository.AddAsync(message);

                    // Need to set as received
                    statusToSet = RequestStatus.Received;
                }
                else
                {
                    // Create request
                    request = new Request()
                    {
                        EducationOrganizationId = educationOrganizationId.Value,
                        RequestManifest = mainfestJson,
                        RequestStatus = RequestStatus.Received,
                        IncomingOutgoing = IncomingOutgoing.Outgoing,
                        Payload = mainfestJson.RequestType
                    };

                    await _requestRepository.AddAsync(request);

                    // Create message
                    message = new Message()
                    {
                        RequestId = request.Id,
                        RequestResponse = RequestResponse.Response,
                        MessageTimestamp = DateTime.UtcNow,
                        MessageContents = JsonDocument.Parse(JsonSerializer.Serialize(request.RequestManifest))
                    };
                    await _messageRepository.AddAsync(message);
                }
            }
            else
            {
                // Create request
                request = new Request()
                {
                    EducationOrganizationId = educationOrganizationId.Value,
                    RequestManifest = mainfestJson,
                    RequestStatus = RequestStatus.Received,
                    IncomingOutgoing = IncomingOutgoing.Outgoing
                };

                await _requestRepository.AddAsync(request);

                // Create message
                message = new Message()
                {
                    RequestId = request.Id,
                    RequestResponse = RequestResponse.Response,
                    MessageContents = JsonDocument.Parse(JsonSerializer.Serialize(request.RequestManifest))
                };
                await _messageRepository.AddAsync(message);
            }

            // Add any attachments
            if (files is not null && files.Count > 0)
            {
                foreach(var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileBlob = await FileHelpers
                            .ProcessFormFile<BufferedSingleFileUploadDb>(file, ModelState, [".png", ".txt", ".pdf", ".json"], 2097152);

                        ManifestContent? fileContentType;

                        if (request is not null && request.EducationOrganizationId == educationOrganizationId && request.ResponseManifest is not null)
                        {
                            fileContentType = request!.ResponseManifest?.Contents?.Where(i => i.FileName == file.FileName).FirstOrDefault();
                        }
                        else
                        {
                            fileContentType = request!.RequestManifest?.Contents?.Where(i => i.FileName == file.FileName).FirstOrDefault();
                        }

                        var messageContent = new PayloadContent()
                        {
                            RequestId = request.Id,
                            MessageId = message!.Id,
                            ContentType = fileContentType?.ContentType,
                            FileName = file.FileName
                        };

                        if (messageContent.ContentType == "application/json")
                        {
                            messageContent.JsonContent = JsonDocument.Parse(System.Text.Encoding.Default.GetString(fileBlob));
                        }
                        else
                        {
                            messageContent.BlobContent = fileBlob;
                        }

                        await _payloadContentRepository.AddAsync(messageContent);
                    }
                    
                }
            }

            if (statusToSet is not null)
            {
                request.RequestStatus = RequestStatus.Received;
                await _requestRepository.UpdateAsync(request);
            }

            return Created("requests", request!.Id.ToString());
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

}