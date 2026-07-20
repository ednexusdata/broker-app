using System.Reflection;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Specifications;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EdNexusData.Broker.Web.Filters;

// The first MVC action filter in this app. A no-op unless the executing action carries
// [LogUserActivity] — see that attribute for how actions opt in. Registered globally in Program.cs so
// new actions only need the attribute, not a change here.
public class UserActivityLogActionFilter : IAsyncActionFilter
{
    private readonly ActivityLogService _activityLogService;
    private readonly ICurrentUser _currentUser;
    private readonly IReadRepository<PayloadContentAction> _payloadContentActionRepository;
    private readonly IReadRepository<Mapping> _mappingRepository;
    private readonly ILogger<UserActivityLogActionFilter> _logger;

    public UserActivityLogActionFilter(
        ActivityLogService activityLogService,
        ICurrentUser currentUser,
        IReadRepository<PayloadContentAction> payloadContentActionRepository,
        IReadRepository<Mapping> mappingRepository,
        ILogger<UserActivityLogActionFilter> logger)
    {
        _activityLogService = activityLogService;
        _currentUser = currentUser;
        _payloadContentActionRepository = payloadContentActionRepository;
        _mappingRepository = mappingRepository;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attribute = (context.ActionDescriptor as ControllerActionDescriptor)?
            .MethodInfo.GetCustomAttribute<LogUserActivityAttribute>();

        var executedContext = await next();

        if (attribute is null) return;
        if (executedContext.Exception is not null && !executedContext.ExceptionHandled) return;

        var userId = _currentUser.AuthenticatedUserId();
        if (userId is null) return;

        try
        {
            var requestId = await ResolveRequestIdAsync(attribute, context);

            await _activityLogService.LogAsync(
                attribute.ActivityType,
                userId,
                attribute.Action,
                attribute.Description ?? attribute.Action,
                requestId,
                ip: context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                userAgent: context.HttpContext.Request.Headers["User-Agent"].ToString());
        }
        catch (Exception ex)
        {
            // A logging failure must never surface as an error for an action that already succeeded.
            _logger.LogWarning(ex, "Failed to write user activity log for {Action}", attribute.Action);
        }
    }

    private async Task<Guid?> ResolveRequestIdAsync(LogUserActivityAttribute attribute, ActionExecutingContext context)
    {
        object? raw = null;
        if (!context.RouteData.Values.TryGetValue(attribute.RouteParamName, out raw))
        {
            context.ActionArguments.TryGetValue(attribute.RouteParamName, out raw);
        }

        if (!Guid.TryParse(raw?.ToString(), out var id)) return null;

        switch (attribute.IdKind)
        {
            case ActivityRequestIdKind.Request:
                return id;

            case ActivityRequestIdKind.PayloadContentAction:
                var action = await _payloadContentActionRepository.FirstOrDefaultAsync(new PayloadContentActionWithPayloadContentId(id));
                return action?.PayloadContent?.RequestId;

            case ActivityRequestIdKind.Mapping:
                var mapping = await _mappingRepository.FirstOrDefaultAsync(new MappingWithPayloadContentId(id));
                return mapping?.PayloadContentAction?.PayloadContent?.RequestId;

            default:
                return null;
        }
    }
}
