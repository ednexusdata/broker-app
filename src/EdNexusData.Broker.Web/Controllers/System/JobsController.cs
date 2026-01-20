// Copyright: 2024 Education Nexus
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Core.Worker;
using System.Linq.Expressions;
using EdNexusData.Broker.Web.Specifications;
using Ardalis.Specification;
using EdNexusData.Broker.Web.Models.Jobs;
using EdNexusData.Broker.Web.Models;
using EdNexusData.Broker.Web.Models.Paginations;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Constants.Claims;
using EdNexusData.Broker.Data;
using Microsoft.EntityFrameworkCore;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize]
public class JobsController : AuthenticatedController<JobsController>
{
    private readonly ILogger<JobsController> logger;
    private readonly IRepository<Job> jobsRepository;
    private readonly IRepository<User> userRepository;
    private readonly CurrentUserHelper currentUserHelper;
    private readonly FocusHelper focusHelper;
    private readonly JobService jobService;
    private readonly BrokerDbContext context;

    public JobsController(
        ILogger<JobsController> logger,
        IRepository<Job> jobsRepository,
        IRepository<User> userRepository,
        CurrentUserHelper currentUserHelper,
        FocusHelper focusHelper,
        JobService jobService,
        BrokerDbContext context)
    {
        this.logger = logger;
        this.jobsRepository = jobsRepository;
        this.userRepository = userRepository;
        this.currentUserHelper = currentUserHelper;
        this.focusHelper = focusHelper;
        this.jobService = jobService;
        this.context = context;
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
            x => x.CreatedBy == currentUserHelper.CurrentUserId()!.Value || x.InitiatedUserId == currentUserHelper.CurrentUserId()!.Value
        };

        searchExpressions = model.BuildSearchExpressions();

        var sortExpression = model.BuildSortExpression();

        var specification = new SearchableWithPaginationSpecification<Job>.Builder(model.Page, model.Size)
            .WithAscending((model.SortDir != null) ? model.IsAscending : false)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(job => job.InitiatedUser)
            )
            .Build();

        var totalItems = await jobsRepository.CountAsync(
            specification,
            cancellationToken);

        var jobs = await jobsRepository.ListAsync(
            specification,
            cancellationToken);

        var jobsViewModels = jobs
            .Select(jobs => new JobViewModel(jobs, currentUserHelper.ResolvedCurrentUserTimeZone()));
        
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

    [Authorize(Policy = CustomClaimType.SystemAdministrator)]
    public async Task<IActionResult> SystemIndex(
        JobModel model,
        CancellationToken cancellationToken,
        Guid? jobId)
    {
        if (jobId is not null)
        {
            ViewBag.JobId = jobId;
        }

        var searchExpressions = new List<Expression<Func<Job, bool>>>();

        if (!focusHelper.IsEdOrgAllFocus()) {
            var focusedEdOrgs = await focusHelper.GetFocusedEdOrgs();
            searchExpressions.Add(
                u => u.CreatedByUser != null && u.CreatedByUser.UserRoles != null && u.CreatedByUser.UserRoles.Any(
                    r => focusedEdOrgs.Contains(r.EducationOrganization!)
                )
            );
            searchExpressions.Add(
                u => u.InitiatedUser != null && u.InitiatedUser.UserRoles != null && u.InitiatedUser.UserRoles.Any(
                    r => focusedEdOrgs.Contains(r.EducationOrganization!)
                )
            );
        }

        searchExpressions = model.BuildSearchExpressions();

        var sortExpression = model.BuildSortExpression();

        var specification = new SearchableWithPaginationSpecification<Job>.Builder(model.Page, model.Size)
            .WithAscending((model.SortDir != null) ? model.IsAscending : false)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(job => job.InitiatedUser)
                .ThenInclude(x => x.UserRoles)
                .Include(x => x.CreatedByUser)
                .ThenInclude(x => x.UserRoles)
            )
            .Build();

        var totalItems = await jobsRepository.CountAsync(
            specification,
            cancellationToken);

        var jobs = await jobsRepository.ListAsync(
            specification,
            cancellationToken);

        var jobsViewModels = jobs
            .Select(jobs => new JobViewModel(jobs, currentUserHelper.ResolvedCurrentUserTimeZone()));
        
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

        return View("Index", result);
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
        var createdJob = await jobService.CreateJobAsync(jobType!, referenceType, job.ReferenceGuid, job.InitiatedUserId, job.JobParameters);

        TempData[VoiceTone.Positive] = $"Restarting job ({job.Id}).";
        return RedirectToAction(nameof(Index), new { JobId = createdJob.Id });
    }

    public async Task<IActionResult> View(Guid id)
    {
        var job = await jobsRepository.GetByIdAsync(id);

        return View(job);
    }
}
