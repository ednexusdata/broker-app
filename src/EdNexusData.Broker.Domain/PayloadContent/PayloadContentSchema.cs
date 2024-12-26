namespace EdNexusData.Broker.Domain;

public class PayloadContentSchema
{
    public string Owner { get; set; } = default!;
    public string Schema { get; set; } = default!;
    public string SchemaVersion { get; set; } = default!;
    public string ObjectType { get; set; } = default!;
    public string ContentObjectType { get; set; } = default!;
    public string ContentType { get; set; } = "application/json";
}