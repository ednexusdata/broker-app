
namespace EdNexusData.Broker.Core.Models;

public class BrokerDnsTxtRecord
{
    public string? Version { get; set; }
    public string? Host { get; set; }
    public string? Path { get; set; }
    public string? KeyAlgorithim { get; set; }
    public string? PublicKey { get; set; }
    public string? Environment { get; set; }

    public string? Scheme { get; set; } = "https";
}