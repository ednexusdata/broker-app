namespace EdNexusData.Broker.Data;

public class DistributedCacheEntry
{
    public string Id { get; set; } = default!;
    public byte[]? Value { get; set; }
    public DateTimeOffset? ExpiresAtTime { get; set; }
    public long SlidingExpirationInSeconds { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
}
