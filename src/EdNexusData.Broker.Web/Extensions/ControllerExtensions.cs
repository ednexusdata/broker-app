using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EdNexusData.Broker.Web;

public static class ControllerExtensions
{
    public static Guid? AuthenticatedUserId(this Controller controller)
    {
        var SessionUserId = controller.User.Claims.Where(v => v.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").FirstOrDefault()?.Value;
        return Guid.Parse(SessionUserId);
    }
}