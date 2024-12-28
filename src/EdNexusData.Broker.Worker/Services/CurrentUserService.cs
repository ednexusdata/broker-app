using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Worker.Services;

public class CurrentUserService : ICurrentUser
{
    private readonly Guid? _workerGuid;

    public CurrentUserService(Guid? workerGuid)
    {
        _workerGuid = workerGuid;
    }

    public CurrentUserService(string workerGuid)
    {
        _workerGuid = Guid.Parse(workerGuid);
    }

    public Guid? AuthenticatedUserId()
    {
        return _workerGuid;
    }
}