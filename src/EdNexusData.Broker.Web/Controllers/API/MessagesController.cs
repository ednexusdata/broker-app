using System.Net;
using System.Text.Json;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Controllers;
using EdNexusData.Broker.Web.Utilities;
using EdNexusData.Broker.Domain.Internal;
using System.Text;

namespace EdNexusData.Broker.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Route("api/v1/messages")]
public class MessagesController : Controller
{
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

          return Created("messages", messageTransmission.RequestId);
        }
      }
      catch(Exception ex)
      {
        return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
      }
    }
}