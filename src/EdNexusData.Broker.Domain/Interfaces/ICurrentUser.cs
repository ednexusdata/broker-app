namespace EdNexusData.Broker.Domain;

public interface ICurrentUser
{
    public Guid? AuthenticatedUserId();
}