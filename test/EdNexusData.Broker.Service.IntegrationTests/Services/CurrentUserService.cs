using EdNexusData.Broker.SharedKernel;

namespace EdNexusData.Broker.Core.IntegrationTests.Services;

public class CurrentUserService : ICurrentUser
{
    public Guid SessionUserId { get; set; }
    
    public CurrentUserService()
    {
        SessionUserId = Guid.NewGuid();
    }

    public Guid? AuthenticatedUserId()
    {
        return SessionUserId;
    }
}