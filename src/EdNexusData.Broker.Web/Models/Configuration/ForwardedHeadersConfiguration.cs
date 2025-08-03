namespace EdNexusData.Broker.Web.Models.Configuration;

public class ForwardedHeadersConfiguration
{
    public List<string> KnownProxies { get; set; } = new();
    public List<NetworkConfiguration> KnownNetworks { get; set; } = new();
}

public class NetworkConfiguration
{
    public string Prefix { get; set; } = default!;
    public int PrefixLength { get; set; }
}