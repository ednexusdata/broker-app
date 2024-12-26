using System.Net;
using System.Text.Json;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Controllers;
using EdNexusData.Broker.Web.Utilities;
using EdNexusData.Broker.Core.Jobs;

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
            var messageTransmission = JsonSerializer.Deserialize<MessageTransmission>(manifest, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true 
            });
            
            var mainfestJson = JsonSerializer.Deserialize<Manifest>(messageTransmission!.Contents!, new JsonSerializerOptions
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
                        Sender = messageTransmission.Sender,
                        SenderSentTimestamp = messageTransmission.SentTimestamp,
                        MessageContents = new MessageContents()
                        {
                            RequestStatus = RequestStatus.Received,
                            Sender = messageTransmission.Sender,
                            SenderSentTimestamp = messageTransmission.SentTimestamp,
                            Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.ResponseManifest))
                        },
                        TransmissionDetails = JsonSerializer.SerializeToDocument(FormatTransmissionMessage(HttpContext)),
                        RequestStatus = RequestStatus.Received
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
                        Sender = messageTransmission.Sender,
                        SenderSentTimestamp = messageTransmission.SentTimestamp,
                        MessageContents = new MessageContents()
                        {     
                            Sender = messageTransmission.Sender,
                            SenderSentTimestamp = messageTransmission.SentTimestamp,
                            Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.RequestManifest)),
                            RequestStatus = RequestStatus.Requested
                        },
                        TransmissionDetails = JsonSerializer.SerializeToDocument(FormatTransmissionMessage(HttpContext)),
                        RequestStatus = RequestStatus.Requested
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
                    Sender = messageTransmission.Sender,
                    SenderSentTimestamp = messageTransmission.SentTimestamp,
                    MessageContents = new MessageContents()
                    {
                        Sender = messageTransmission.Sender,
                        SenderSentTimestamp = messageTransmission.SentTimestamp,
                        Contents = JsonDocument.Parse(JsonSerializer.Serialize(request.RequestManifest)),
                        RequestStatus = RequestStatus.Requested
                    },
                    TransmissionDetails = JsonSerializer.SerializeToDocument(FormatTransmissionMessage(HttpContext)),
                    RequestStatus = RequestStatus.Requested
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

            // create message response and return it
            var returnMessageContent = new MessageContents()
            {
                SenderSentTimestamp = DateTimeOffset.UtcNow,
                Sender = new EducationOrganizationContact()
                {
                    Name = "Broker " +  Dns.GetHostName()
                },
                MessageText = string.Format("Request received with request id: {0}", request!.Id.ToString()),
                Contents = JsonSerializer.SerializeToDocument(request!.Id.ToString()),
                RequestStatus = RequestStatus.Received
            };
            var returnMessage = new Message()
            {
                Request = request,
                Sender = returnMessageContent.Sender,
                MessageTimestamp = DateTime.UtcNow,
                SenderSentTimestamp = DateTime.UtcNow,
                RequestStatus = RequestStatus.Received,
                MessageContents = returnMessageContent
            };
            await _messageRepository.AddAsync(returnMessage);

            return Created("requests", returnMessageContent);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

    private TransmissionMessage FormatTransmissionMessage(HttpContext http)
    {
        var requestContent = new TransmissionContent()
        {
            Headers = http.Request.Headers.ToDictionary(x => x.Key, y => y.Value.AsEnumerable() )!
        };

        var responseContent = new TransmissionContent()
        {
            StatusCode = Enum.Parse<HttpStatusCode>(http.Response.StatusCode.ToString()),
            Headers = http.Response.Headers.ToDictionary(x => x.Key, y => y.Value.AsEnumerable() )!
        };

        return new TransmissionMessage()
        {
            Request = requestContent,
            Response = responseContent
        };
    }

}