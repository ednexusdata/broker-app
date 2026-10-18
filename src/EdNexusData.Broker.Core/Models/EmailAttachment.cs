namespace EdNexusData.Broker.Core.Models;

public class EmailAttachment
{
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public byte[] Content { get; set; } = default!;
}
