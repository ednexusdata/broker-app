using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Web.Helpers;

namespace EdNexusData.Broker.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/v1/jobs")]
public class ApiJobsController : Controller
{
    private readonly JobStatusService<ApiJobsController> _jobStatusService;
    private readonly FocusHelper focusHelper;

    public ApiJobsController(JobStatusService<ApiJobsController> jobStatusService, FocusHelper focusHelper)
    {
        _jobStatusService = jobStatusService;
        this.focusHelper = focusHelper;
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
            List<EducationOrganization>? focusedEdOrgs = null;

            if (!focusHelper.IsEdOrgAllFocus()) {
                focusedEdOrgs = await focusHelper.GetFocusedEdOrgs();
            }
            
            var results = await _jobStatusService.Get(jobId.Value, focusedEdOrgs);

            return Ok(results);
        }
        catch(Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, $"{ex.Message}\n\n{ex.StackTrace}");
        }
    }

}
