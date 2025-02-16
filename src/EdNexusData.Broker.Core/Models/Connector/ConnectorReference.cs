namespace EdNexusData.Broker.Core.Models;

public class ConnectorReference : BaseEntity, IAggregateRoot
{
    public string Reference { get; set; } = default!;
    public string Version { get; set; } = default!;
}