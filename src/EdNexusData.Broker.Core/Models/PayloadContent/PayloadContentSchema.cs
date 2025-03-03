namespace EdNexusData.Broker.Core;

public class PayloadContentSchema
{
    public string Owner { get; set; } = default!;
    public string Schema { get; set; } = default!;
    public string SchemaVersion { get; set; } = default!;
    public string ObjectType { get; set; } = default!;
    public string ContentObjectType { get; set; } = default!;
    public string ContentType { get; set; } = "application/json";

    public static PayloadContentSchema ToCore(Common.PayloadContents.PayloadContentSchema payloadContentSchema)
    {
        return new PayloadContentSchema()
        {
            Owner = payloadContentSchema.Owner,
            Schema = payloadContentSchema.Schema,
            SchemaVersion = payloadContentSchema.SchemaVersion,
            ObjectType = payloadContentSchema.ObjectType,
            ContentObjectType = payloadContentSchema.ContentObjectType,
            ContentType = payloadContentSchema.ContentType
        };
    }
}