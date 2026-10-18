using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Web.Filters;

// Declares that an action's success should be recorded to ActivityLogs. The actual write happens
// in UserActivityLogActionFilter, registered globally — this attribute is a no-op by itself.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class LogUserActivityAttribute : Attribute
{
    public ActivityType ActivityType { get; }

    // Stable machine key, e.g. "Requests.View", "Mapping.Import".
    public string Action { get; }

    // Human-readable text for the admin UI. Defaults to Action if not set.
    public string? Description { get; init; }

    // Name of the route value / action argument holding the id to resolve a RequestId from.
    public string RouteParamName { get; init; } = "id";

    public ActivityRequestIdKind IdKind { get; init; } = ActivityRequestIdKind.Request;

    public LogUserActivityAttribute(ActivityType activityType, string action)
    {
        ActivityType = activityType;
        Action = action;
    }
}
