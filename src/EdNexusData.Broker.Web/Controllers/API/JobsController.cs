using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Core.Worker;

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
        if (jobId.HasValue == false)
        {
            return BadRequest("Job ID is required.");
        }

        try
        {
            var results = await _jobStatusService.Get(jobId.Value);

            return Ok(results);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

}
