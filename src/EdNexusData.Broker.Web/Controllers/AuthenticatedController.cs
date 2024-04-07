using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Data;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using static EdNexusData.Broker.Web.Constants.Sessions.SessionKey;

namespace EdNexusData.Broker.Web.Controllers;

public class AuthenticatedController<T> : Controller where T : AuthenticatedController<T>
{
    private IReadRepository<EducationOrganization> _edOrgRepo => HttpContext.RequestServices.GetService<IReadRepository<EducationOrganization>>()!;

    public AuthenticatedController()
    {
    }

    protected void RefreshSession()
    {
        HttpContext.Session.SetString("LastAccessed", DateTime.UtcNow.ToString());
    }

    protected void ValidateSession()
    {
        if(HttpContext is not null && !HttpContext.Request.Path.StartsWithSegments("/login/logout"))
        {
            HttpContext.Response.Redirect("/login/logout");
            return;
        }
    }

    protected Guid GetFocusOrganizationId()
    {
        var session = HttpContext?.Session;
        _ = Guid.TryParse(session?.GetString(FocusOrganizationKey), out var focusOrganizationKey);
        return focusOrganizationKey;
    }


    protected string GetFocusOrganizationDistrict()
    {
        var session = HttpContext?.Session;
        return session is null ? string.Empty : session.GetString(FocusOrganizationDistrict) ?? string.Empty;
    }
    
    protected string GetFocusOrganizationSchool()
    {
        var session = HttpContext?.Session;
        return session is null ? string.Empty : session.GetString(FocusOrganizationSchool) ?? string.Empty;
    }
}
