using System.Text.Json;

namespace EdNexusData.Broker.Domain;

public class MessageTransmission
{
    public EducationOrganizationContact? Sender { get; set; }
    public DateTimeOffset? SentTimestamp { get; set; }
    public JsonDocument? Contents { get; set; }
}