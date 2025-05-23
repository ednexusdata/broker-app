using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Utilities;
using EdNexusData.Broker.Core.Services;

namespace EdNexusData.Broker.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Route("api/v1/requests")]
public class RequestsController : Controller
{
    private readonly ReceiveMessageService receiveMessageService;

    public RequestsController(
        ReceiveMessageService receiveMessageService
    )
    {
        this.receiveMessageService = receiveMessageService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromForm] string manifest, [FromForm] IList<IFormFile> files)
    {
        // Process attachments
        var filesToProcess = await FileHelpers.ToFiles(files, ModelState);
        
        try
        {
            var returnMessageContent = await receiveMessageService.ReceiveRequest(manifest, filesToProcess, HttpContext.Response);
            return Created("requests", returnMessageContent);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}\n\n{ex.InnerException?.Message}\n\n{ex.InnerException?.StackTrace}\n\nManifest sent:{manifest}");
        }
    }
}