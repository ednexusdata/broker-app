using System.Text.Json;
using EdNexusData.Broker.Core.Jobs;

namespace EdNexusData.Broker.Domain;

public class MessageContents
{
    public Guid? RequestId { get; set; }
    public RequestStatus? RequestStatus { get; set; }
    public EducationOrganizationContact? Sender { get; set; }
    public DateTimeOffset? SenderSentTimestamp { get; set; }
    public JsonDocument? Contents { get; set; }
    public string? MessageText { get; set; }
}