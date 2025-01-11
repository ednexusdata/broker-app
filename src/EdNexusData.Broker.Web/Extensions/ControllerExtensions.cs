using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EdNexusData.Broker.Web;

public static class ControllerExtensions
{
    public static Guid? AuthenticatedUserId(this Controller controller)
    {
        var SessionUserId = controller.User.Claims.Where(v => v.Type == ClaimConstants.NameIdentifierId).FirstOrDefault()?.Value;

        if (Guid.TryParse(SessionUserId, out Guid idToSend))
        {
            return idToSend;
        }

        return null;
    }
}