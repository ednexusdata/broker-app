namespace EdNexusData.Broker.Core.Models;

public class StoreConnector
{
    public string? ReferenceName { get; set; }
    public string? Company { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public List<StoreConnectorRelease>? Releases { get; set; }

    public StoreConnectorRelease? LatestRelease()
    {
        return Releases?
            .OrderByDescending(v => v.ToSystemVersion())
            .First();
    }

    public ConnectorReference ToConnectorReference()
    {
        return new ConnectorReference()
        {
            Reference = ReferenceName!,
            Version = "*"
        };
    }
}

public class StoreConnectorRelease
{
    public string? Version { get; set; }
    public string? Url { get; set; }

    public Version? ToSystemVersion()
    {
        return (Version is not null) ? new Version(Version) : null;
    }
}