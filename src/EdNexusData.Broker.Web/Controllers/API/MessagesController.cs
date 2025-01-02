using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using EdNexusData.Broker.Core.Interfaces;
using EdNexusData.Broker.Core.Services;

namespace EdNexusData.Broker.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Route("api/v1/messages")]
public class MessagesController : Controller
{
    private readonly INowWrapper nowWrapper;
    private readonly MessageService messageService;

    public MessagesController(INowWrapper nowWrapper, MessageService messageService)
    {
        this.nowWrapper = nowWrapper;
        this.messageService = messageService;
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
                var message = await messageService.CreateFromAPIRequest(rawValue);
                return Created("messages", message.RequestId);
            }
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }
}