using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Web.Helpers;

public class CurrentUserHelper
{
    private readonly ILogger<CurrentUserHelper> logger;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ISession session;

    public CurrentUserHelper(
        ILogger<CurrentUserHelper> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        this.logger = logger;
        this.httpContextAccessor = httpContextAccessor;
        session = httpContextAccessor!.HttpContext?.Session!;
    }

    public User? CurrentUser()
    {
        return session?.GetObjectFromJson<User>("User.Current");
    }

    public Guid? CurrentUserId()
    {
        var currentUser = CurrentUser();
        return currentUser?.Id;
    }

    public TimeZoneInfo? CurrentUserTimeZone()
    {
        if (CurrentUser()?.TimeZone is not null)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(CurrentUser()?.TimeZone!);
        }
        return null;
    }

    public TimeZoneInfo ResolvedCurrentUserTimeZone()
    {
        if (CurrentUserTimeZone() is not null)
        {
            return CurrentUserTimeZone()!;
        }
        
        return TimeZoneInfo.Local;
    }
}