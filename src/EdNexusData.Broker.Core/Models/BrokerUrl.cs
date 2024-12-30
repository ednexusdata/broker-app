namespace EdNexusData.Broker.Core.Models;

public class BrokerUrl
{
    public string Host { get; set; } = default!;
    public string Path { get; set; } = default!;

    public Uri HostToUri()
    {
        return new Uri($"https://{Host}");
    }
}