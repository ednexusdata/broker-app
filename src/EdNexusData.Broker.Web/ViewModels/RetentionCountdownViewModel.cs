using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Web.ViewModels;

public class RetentionCountdownViewModel
{
    public DateTimeOffset LastActivityAt { get; set; }
    public DateTimeOffset DestructionAt { get; set; }
    public string DestructionAtIso => DestructionAt.ToString("O");

    public static RetentionCountdownViewModel? FromRequest(Request? request, int cleanupDays)
    {
        if (request is null) return null;

        var lastActivityAt = request.UpdatedAt ?? request.CreatedAt;

        return new RetentionCountdownViewModel()
        {
            LastActivityAt = lastActivityAt,
            DestructionAt = lastActivityAt.AddDays(cleanupDays)
        };
    }
}
