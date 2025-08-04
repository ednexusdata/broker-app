namespace EdNexusData.Broker.Data;

public class DistributedCacheEntry
{
    public Guid? Id { get; set; }
    public byte[]? Value { get; set; }
    public DateTimeOffset? ExpiresAtTime { get; set; }
    public DateTimeOffset? SlidingExpirationInSeconds { get; set; }
    public DateTimeOffset? AbsoluteExpiration { get; set; }
}
