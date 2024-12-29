using EdNexusData.Broker.Core.Interfaces;

namespace EdNexusData.Broker.Core;

public class NowWrapper : INowWrapper
{
    public DateTimeOffset UtcNow => DateTime.UtcNow;
}
