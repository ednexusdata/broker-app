using EdNexusData.Broker.SharedKernel;
using Microsoft.Identity.Web;

namespace EdNexusData.Broker.Web.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContext;

    public CurrentUserService(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public Guid? AuthenticatedUserId()
    {
        var SessionUserId = _httpContext.HttpContext!.User.Claims.Where(v => v.Type == ClaimConstants.NameIdentifierId).FirstOrDefault()?.Value;
        if (Guid.TryParse(SessionUserId, out var sessionId))
        {
            return sessionId;
        }
        return null;
    }
}