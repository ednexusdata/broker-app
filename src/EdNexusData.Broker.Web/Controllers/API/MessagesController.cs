using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using EdNexusData.Broker.Core.Interfaces;

namespace EdNexusData.Broker.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Route("api/v1/messages")]
public class MessagesController : Controller
{
    private readonly INowWrapper nowWrapper;
    private readonly IRepository<Message> messageRepository;

    public MessagesController(INowWrapper nowWrapper, IRepository<Message> messageRepository)
    {
        this.nowWrapper = nowWrapper;
        this.messageRepository = messageRepository;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        try
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string rawValue = await reader.ReadToEndAsync();

                var messageTransmission = JsonSerializer.Deserialize<MessageContents>(rawValue, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true 
                });

                var message = new Message()
                {
                    RequestId = messageTransmission!.RequestId!.Value,
                    RequestResponse = RequestResponse.Response,
                    MessageTimestamp = nowWrapper.UtcNow,
                    Sender = messageTransmission.Sender,
                    SenderSentTimestamp = messageTransmission.SenderSentTimestamp,
                    MessageContents = messageTransmission,
                    RequestStatus = messageTransmission.RequestStatus,
                    MessageStatus = Core.Messages.MessageStatus.Received
                };
                await messageRepository.AddAsync(message);

                return Created("messages", messageTransmission.RequestId);
            }
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }
}