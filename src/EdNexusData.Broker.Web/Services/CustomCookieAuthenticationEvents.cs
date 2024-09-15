using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EdNexusData.Broker.Web;

public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private const string TicketIssuedTicks = nameof(TicketIssuedTicks);

    public override async Task SigningIn(CookieSigningInContext context)
    {
        context.Properties.SetString(
            TicketIssuedTicks,
            DateTimeOffset.UtcNow.Ticks.ToString());

        await base.SigningIn(context);
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