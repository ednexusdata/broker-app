using EdNexusData.Broker.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using static EdNexusData.Broker.Web.Constants.Sessions.SessionKey;

namespace EdNexusData.Broker.Web.Middleware;

public class ActiveBrokerSessionMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ActiveBrokerSessionMiddleware> logger;

    public ActiveBrokerSessionMiddleware(
        RequestDelegate next, 
        ILogger<ActiveBrokerSessionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint != null)
        {
            var isAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null;

            if (!isAnonymous)
            {
                bool isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;

                bool hasSession = !string.IsNullOrEmpty(context.Session.GetString(UserCurrent));

                if (!isAuthenticated || !hasSession)
                {
                    logger.LogInformation("User session is inactive or missing. Redirecting to login.");
                    await SessionHelper.InvalidateUserSessionAsync(context);
                    context.Response.Cookies.Delete("EdNexusData.Broker.Identity");
                    context.Response.Cookies.Delete("EdNexusData.Broker.Session");
                    context.Response.Redirect("/");
                    return;
                }
            }
            logger.LogInformation("In anonymous endpoint.");
        }

        await next(context);
    }
}