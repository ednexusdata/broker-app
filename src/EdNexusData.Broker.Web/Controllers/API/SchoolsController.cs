using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Service.Lookup;

namespace EdNexusData.Broker.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/v1/schools")]
public class SchoolsController : Controller
{
    private readonly DirectoryLookupService _directoryLookupService;

    public SchoolsController(DirectoryLookupService directoryLookupService)
    {
        _directoryLookupService = directoryLookupService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search([FromQuery] string domain)
    {
        try
        {
            var results = await _directoryLookupService.SearchAsync(domain);

            return Ok(results);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

}
