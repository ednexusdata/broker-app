// Copyright: 2024 Education Nexus
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Domain.Worker;
using System.Linq.Expressions;
using EdNexusData.Broker.Web.Specifications;
using Ardalis.Specification;
using EdNexusData.Broker.Web.Models.Jobs;
using EdNexusData.Broker.Web.Models;
using EdNexusData.Broker.Web.Models.Paginations;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Service;
using EdNexusData.Broker.Web.Constants.DesignSystems;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize]
public class JobsController : AuthenticatedController<JobsController>
{
    private readonly ILogger<JobsController> logger;
    private readonly IRepository<Job> jobsRepository;
    private readonly CurrentUserHelper currentUserHelper;
    private readonly JobService jobService;

    public JobsController(
        ILogger<JobsController> logger,
        IRepository<Job> jobsRepository,
        CurrentUserHelper currentUserHelper,
        JobService jobService)
    {
        this.logger = logger;
        this.jobsRepository = jobsRepository;
        this.currentUserHelper = currentUserHelper;
        this.jobService = jobService;
    }

    public async Task<IActionResult> Index(
        JobModel model,
        CancellationToken cancellationToken,
        Guid? jobId)
    {
        if (jobId is not null)
        {
            ViewBag.JobId = jobId;
        }

        var searchExpressions = new List<Expression<Func<Job, bool>>>
        {
            // Must restrict to currently logged in user
            x => x.CreatedBy == currentUserHelper.CurrentUserId()!.Value
        };

        searchExpressions = model.BuildSearchExpressions();

        var sortExpression = model.BuildSortExpression();

        var specification = new SearchableWithPaginationSpecification<Job>.Builder(model.Page, model.Size)
            .WithAscending((model.SortDir != null) ? model.IsAscending : false)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .Build();

        var totalItems = await jobsRepository.CountAsync(
            specification,
            cancellationToken);

        var jobs = await jobsRepository.ListAsync(
            specification,
            cancellationToken);

        var jobsViewModels = jobs
            .Select(jobs => new JobViewModel(jobs));
        
        if (!string.IsNullOrWhiteSpace(model.SearchBy))
        {
            // jobsViewModels = jobsViewModels
            //     .Where(request => request.Student?.ToLower().Contains(model.SearchBy) is true || request.ReleasingDistrict.ToLower().Contains(model.SearchBy)
            //      || request.ReleasingSchool.ToLower().Contains(model.SearchBy));
            totalItems = jobsViewModels.Count();
        }

        var result = new PaginatedViewModel<JobViewModel>(
            jobsViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restart(Guid id)
    {
        var job = await jobsRepository.GetByIdAsync(id);

        Guard.Against.Null(job, "job", "Unable to find job for Id");

        var jobType = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetExportedTypes())
                            .Where(p => p.FullName == job.JobType!).FirstOrDefault();

        Type? referenceType = null; 
        if (job.ReferenceType is not null)
        {
            referenceType = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetExportedTypes())
                            .Where(p => p.FullName == job.ReferenceType!).FirstOrDefault();
        }
        
        // Create job
        var createdJob = await jobService.CreateJobAsync(jobType!, referenceType, job.ReferenceGuid);

        TempData[VoiceTone.Positive] = $"Restarting job ({job.Id}).";
        return RedirectToAction(nameof(Index), new { JobId = createdJob.Id });
    }

    public async Task<IActionResult> View(Guid id)
    {
        var job = await jobsRepository.GetByIdAsync(id);

        return View(job);
    }
}
