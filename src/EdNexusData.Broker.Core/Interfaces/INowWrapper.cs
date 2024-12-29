namespace EdNexusData.Broker.Core.Interfaces;

public interface INowWrapper
{
    DateTimeOffset UtcNow { get; }
}
