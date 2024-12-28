namespace EdNexusData.Broker.Core;

public interface ICurrentUser
{
    public Guid? AuthenticatedUserId();
}