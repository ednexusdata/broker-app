using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace EdNexusData.Broker.Web.Helpers;

public class SessionHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task InvalidateUserSessionAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        // 1. Clear all session data
        context.Session.Clear();

        // 2. Sign out of the built-in authentication framework (Cookies/Identity)
        // This removes the auth cookie from the user's browser
        await context.SignOutAsync();
    }

    public static async Task InvalidateUserSessionAsync(IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return;

        // 1. Clear all session data
        context.Session.Clear();

        // 2. Sign out of the built-in authentication framework (Cookies/Identity)
        // This removes the auth cookie from the user's browser
        await context.SignOutAsync();

        context.User = new ClaimsPrincipal(new ClaimsIdentity());
    }

    public static async Task InvalidateUserSessionAsync(HttpContext context)
    {
        if (context == null) return;

        // 1. Clear all session data
        context.Session.Clear();

        // 2. Sign out of the built-in authentication framework (Cookies/Identity)
        // This removes the auth cookie from the user's browser
        await context.SignOutAsync();

        context.User = new ClaimsPrincipal(new ClaimsIdentity());
    }
}