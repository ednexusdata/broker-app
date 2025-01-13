using System.Text.Json;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core;

public class MessageContents
{
    public Guid? RequestId { get; set; }
    public Guid? EducationOrganizationId { get; set; }
    public RequestStatus? RequestStatus { get; set; }
    public EducationOrganizationContact? Sender { get; set; }
    public DateTimeOffset? SenderSentTimestamp { get; set; }
    public JsonDocument? Contents { get; set; }
    public string? MessageText { get; set; }
    public string? MessageType { get; set; }
}