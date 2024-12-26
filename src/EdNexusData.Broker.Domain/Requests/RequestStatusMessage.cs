using EdNexusData.Broker.Core.Jobs;

namespace EdNexusData.Broker.Domain;

public class RequestStatusUpdateMessage
{
    public Guid? RequestId { get; set; }
    public string MessageType { get; set; } = typeof(RequestStatusUpdateMessage).FullName!;
    public RequestStatus? RequestStatus { get; set; }
    public EducationOrganizationContact? SenderContact { get; set; }
    public DateTimeOffset? SentTimestamp { get; set; }
    public string? Note { get; set; }
    public MatchDisposition? MatchDisposition { get; set; }
    public bool? Open { get; set; }
}