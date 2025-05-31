namespace EdNexusData.Broker.Core;

public class Setting : BaseEntity, IAggregateRoot
{
    public string? Key { get; set; }
    public string? Value { get; set; }
}