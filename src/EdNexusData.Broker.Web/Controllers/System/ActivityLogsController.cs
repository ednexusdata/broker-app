using Ardalis.Specification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.Specifications;
using EdNexusData.Broker.Web.Models.ActivityLogs;
using EdNexusData.Broker.Web.Models.Paginations;
using Microsoft.EntityFrameworkCore;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = SystemAdministrator)]
public class ActivityLogsController : AuthenticatedController<ActivityLogsController>
{
    private readonly IRepository<ActivityLog> activityLogsRepository;
    private readonly CurrentUserHelper currentUserHelper;

    public ActivityLogsController(
        IRepository<ActivityLog> activityLogsRepository,
        CurrentUserHelper currentUserHelper)
    {
        this.activityLogsRepository = activityLogsRepository;
        this.currentUserHelper = currentUserHelper;
    }

    public async Task<IActionResult> SystemIndex(
        ActivityLogModel model,
        CancellationToken cancellationToken)
    {
        var searchExpressions = model.BuildSearchExpressions();
        var sortExpression = model.BuildSortExpression();

        var specification = new SearchableWithPaginationSpecification<ActivityLog>.Builder(model.Page, model.Size)
            .WithAscending((model.SortDir != null) ? model.IsAscending : false)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(log => log.User)
            )
            .Build();

        var totalItems = await activityLogsRepository.CountAsync(specification, cancellationToken);
        var logs = await activityLogsRepository.ListAsync(specification, cancellationToken);

        var logViewModels = logs
            .Select(log => new ActivityLogViewModel(log, currentUserHelper.ResolvedCurrentUserTimeZone()));

        var result = new PaginatedViewModel<ActivityLogViewModel>(
            logViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return View(result);
    }
}
