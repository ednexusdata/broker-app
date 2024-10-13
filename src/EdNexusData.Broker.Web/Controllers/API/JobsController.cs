using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Service.Worker;

namespace EdNexusData.Broker.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/v1/jobs")]
public class ApiJobsController : Controller
{
    private readonly JobStatusService<ApiJobsController> _jobStatusService;

    public ApiJobsController(JobStatusService<ApiJobsController> jobStatusService)
    {
        _jobStatusService = jobStatusService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? jobId)
    {
        try
        {
            var results = await _jobStatusService.Get(jobId);

            return Ok(results);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

}
