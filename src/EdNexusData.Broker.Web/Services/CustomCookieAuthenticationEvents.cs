using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Services;

namespace EdNexusData.Broker.Web;

public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private const string TicketIssuedTicks = nameof(TicketIssuedTicks);

    private readonly ActivityLogService _userActivityLogService;
    private readonly ILogger<CustomCookieAuthenticationEvents> _logger;

    public CustomCookieAuthenticationEvents(
        ActivityLogService userActivityLogService,
        ILogger<CustomCookieAuthenticationEvents> logger)
    {
        _userActivityLogService = userActivityLogService;
        _logger = logger;
    }

    public override async Task SigningIn(CookieSigningInContext context)
    {
        context.Properties.SetString(
            TicketIssuedTicks,
            DateTimeOffset.UtcNow.Ticks.ToString());

        await LogLoginAsync(context);

        await base.SigningIn(context);
    }

    // All four sign-in code paths (password+TOTP, dev-only anonymous, OAuth callback, custom SSO
    // connector) funnel through here regardless of which one fired, so this is the single place to log
    // a successful login rather than touching each of them individually. Deliberately does not use
    // ICurrentUser/CurrentUserHelper: ICurrentUser reads HttpContext.User, which for this request is
    // still the pre-sign-in (anonymous) principal, and CurrentUserHelper reads a session value that
    // LoginController only sets *after* SignInAsync returns — neither is populated yet at this point.
    private async Task LogLoginAsync(CookieSigningInContext context)
    {
        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return;

        var (action, description) = ResolveLoginMethod(context.HttpContext.Request);

        try
        {
            await _userActivityLogService.LogAsync(
                ActivityType.Login,
                userId,
                action,
                description,
                requestId: null,
                ip: context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                userAgent: context.HttpContext.Request.Headers["User-Agent"].ToString());
        }
        catch (Exception ex)
        {
            // A logging failure must never block sign-in.
            _logger.LogWarning(ex, "Failed to write login activity log for user {UserId}", userId);
        }
    }

    private static (string Action, string Description) ResolveLoginMethod(HttpRequest request)
    {
        if (request.Path.StartsWithSegments("/login/anonymous"))
        {
            return ("Login.Anonymous", "Signed in (anonymous/dev login)");
        }
        if (request.Path.StartsWithSegments("/login/externallogin"))
        {
            return ("Login.OAuth", "Signed in via OAuth");
        }
        if (request.Path.StartsWithSegments("/login/connector"))
        {
            return ("Login.SSO", $"Signed in via {request.RouteValues["provider"]} SSO");
        }
        return ("Login.Password", "Signed in with email/password");
    }

    public override async Task ValidatePrincipal(
        CookieValidatePrincipalContext context)
    {
        var ticketIssuedTicksValue = context
            .Properties.GetString(TicketIssuedTicks);

        if (ticketIssuedTicksValue is null ||
            !long.TryParse(ticketIssuedTicksValue, out var ticketIssuedTicks))
        {
            await RejectPrincipalAsync(context);
            return;
        }

        var ticketIssuedUtc =
            new DateTimeOffset(ticketIssuedTicks, TimeSpan.FromHours(0));

        if (DateTimeOffset.UtcNow - ticketIssuedUtc > TimeSpan.FromHours(8))
        {
            await RejectPrincipalAsync(context);
            return;
        }

        await base.ValidatePrincipal(context);
    }

    private static async Task RejectPrincipalAsync(
        CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync();
    }
}
