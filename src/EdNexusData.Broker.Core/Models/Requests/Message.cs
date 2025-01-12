using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Messages;

namespace EdNexusData.Broker.Core;

public class Message : BaseEntity, IAggregateRoot
{
    public Guid RequestId { get; set; }
    public Request? Request { get; set; }
    public RequestResponse RequestResponse { get; set; } = RequestResponse.Request;
    public string? MessageType { get; set; }
    public DateTimeOffset? MessageTimestamp { get; set; }
    public DateTimeOffset? SentTimestamp { get; set; }
    public MessageContents? MessageContents { get; set; }
    public TransmissionMessage? TransmissionDetails { get; set; }
    public RequestStatus? RequestStatus { get; set; }
    public MessageStatus? MessageStatus { get; set; }

    public List<PayloadContent>? PayloadContents { get; set; }
}