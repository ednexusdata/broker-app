// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using System.Text.Json;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Messages;

namespace EdNexusData.Broker.Core;

public class Message : BaseEntity, IAggregateRoot
{
    public Guid RequestId { get; set; }
    public Request? Request { get; set; }
    public RequestResponse RequestResponse { get; set; } = RequestResponse.Request;
    public DateTimeOffset? MessageTimestamp { get; set; }
    public EducationOrganizationContact? Sender { get; set; }
    public DateTimeOffset? SenderSentTimestamp { get; set; }
    public MessageContents? MessageContents { get; set; }
    public JsonDocument? TransmissionDetails { get; set; }
    public List<PayloadContent>? PayloadContents { get; set; }
    public RequestStatus? RequestStatus { get; set; }
    public MessageStatus? MessageStatus { get; set; }
}