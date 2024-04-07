using EdNexusData.Broker.Web.Services;

namespace EdNexusData.Broker.Web;

public class ScopedHttpContextMiddleware 
{
    private readonly RequestDelegate _next;

    public ScopedHttpContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context, ScopedHttpContext scopedContext)
    {
        scopedContext.HttpContext = context;
        return _next(context);
    }
}