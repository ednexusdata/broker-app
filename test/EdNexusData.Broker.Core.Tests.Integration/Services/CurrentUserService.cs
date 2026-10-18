using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Core.Tests.Integration.Services;

public class CurrentUserService : ICurrentUser
{
    // Null by default: entities created before a real user exists (e.g. the seeded user itself) get a
    // null CreatedBy rather than a stamp pointing at a Guid that violates the self-referencing FK on Users.
    // Tests that need a "logged in as" context can set this to a real, already-seeded user's Id.
    public Guid? SessionUserId { get; set; }

    public Guid? AuthenticatedUserId()
    {
        return SessionUserId;
    }
}
