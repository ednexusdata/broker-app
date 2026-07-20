namespace EdNexusData.Broker.Web.Filters;

public enum ActivityRequestIdKind
{
    // The route/action-argument value is the Request's Id directly.
    Request,

    // The value is a PayloadContentAction.Id; resolve via .PayloadContent.RequestId.
    PayloadContentAction,

    // The value is a Mapping.Id; resolve via .PayloadContentAction.PayloadContent.RequestId.
    Mapping
}
