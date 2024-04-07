using EdNexusData.Broker.Connector;

namespace EdNexusData.Broker.Service;

public class PayloadContentTypeDisplay
{
    public string DisplayName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public bool AllowMultiple { get; set; }
    public bool AllowConfiguration { get; set; }
}