// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Text.Json;
using System.Xml.Linq;

namespace EdNexusData.Broker.Core;

public class PayloadContent : BaseEntity, IAggregateRoot
{
    public Guid RequestId { get; set; }
    public Request? Request { get; set; }
    public Guid? MessageId { get; set; }
    public Message? Message { get; set; }
    public string? ContentType { get; set; }
    public string? FileName { get; set; }
    public byte[]? BlobContent { get; set; }
    public JsonDocument? JsonContent { get; set; }
    public XElement? XmlContent { get; set; }
    public List<PayloadContentAction>? PayloadContentActions { get; set; }
}