using EdNexusData.Broker.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using static EdNexusData.Broker.Web.Constants.Sessions.SessionKey;

namespace EdNexusData.Broker.Web.Authorization;

public class RecordAllowedRequirement : IAuthorizationRequirement
{
}

public class RecordAuthorizationHandler : AuthorizationHandler<RecordAllowedRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IReadRepository<Request> requestRepository;
    private readonly IReadRepository<EducationOrganization> educationOrganizationRepository;

    public RecordAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        IReadRepository<Request> requestRepository,
        IReadRepository<EducationOrganization> educationOrganizationRepository
    )
    {
        _httpContextAccessor = httpContextAccessor;
        this.requestRepository = requestRepository;
        this.educationOrganizationRepository = educationOrganizationRepository;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RecordAllowedRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null) 
            throw new NullReferenceException("HttpContext is null in the RecordAuthorizationHandler.");
        
        // Extract route data 
        var routeData = httpContext.GetRouteData(); 
        var controller = routeData.Values["controller"]?.ToString(); 
        var action = routeData.Values["action"]?.ToString(); 
        var id = routeData.Values["id"]?.ToString();
        
        // Get current focus
        var currentFocus = await FocusHelper.GetFocusedEdOrgs(httpContext.Session, educationOrganizationRepository);

        switch (controller)
        {
            case "Incoming" when action != "Index" && id != null:
            case "Outgoing" when action != "Index" && id != null:
            case "Preparing" when id != null:
                // Fetch the request from the repository
                var request = await requestRepository.GetByIdAsync(Guid.Parse(id));
                if (request != null)
                {
                    // Check if the request's EducationOrganizationId matches the current focus
                    if (currentFocus.Any(x => x.Id == request.EducationOrganizationId))
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                }
                break;
            // Add more cases as needed for other controllers/actions
        }

        context.Succeed(requirement);
    }
}
