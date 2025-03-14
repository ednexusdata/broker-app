using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Controllers;
using EdNexusData.Broker.Core.Lookup;

namespace EdNexusData.Broker.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/v1/students")]
public class StudentsSearchController : AuthenticatedController<StudentsSearchController>
{
    private readonly StudentLookupService _studentLookupService;

    public StudentsSearchController(IHttpContextAccessor httpContextAccessor, StudentLookupService studentLookupService)
    {
        _studentLookupService = studentLookupService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search([FromQuery] string search, [FromQuery] PayloadDirection payloadDirection)
    {
        try
        {
            var results = await _studentLookupService.SearchAsync(payloadDirection, search);

            return Ok(results);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

}
