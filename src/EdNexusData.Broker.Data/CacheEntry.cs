using System.ComponentModel.DataAnnotations;

namespace EdNexusData.Broker.Data;

public class CacheEntry
{
    [Key]
    public string Id { get; set; } = default!;

    public byte[]? Value { get; set; }

    public DateTimeOffset? AbsoluteExpiration { get; set; }

    public DateTimeOffset? SlidingExpiration { get; set; }

    public DateTimeOffset? LastAccessed { get; set; }
}