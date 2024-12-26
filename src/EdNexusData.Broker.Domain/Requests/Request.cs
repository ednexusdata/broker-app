// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using EdNexusData.Broker.Core.Jobs;

namespace EdNexusData.Broker.Domain;

public class Request : BaseEntity, IAggregateRoot
{
    public Guid EducationOrganizationId { get; set; }
    public StudentRequest? Student { get; set; }
    public string Payload { get; set; } = default!;
    public Guid? RequestProcessUserId { get; set; }
    public DateTimeOffset? InitialRequestSentDate { get; set; }
    public Manifest? RequestManifest { get; set; }
    public Manifest? ResponseManifest { get; set; }
    public Guid? ResponseProcessUserId { get; set; }
    public IncomingOutgoing IncomingOutgoing { get; set; } = IncomingOutgoing.Incoming;
    public RequestStatus RequestStatus { get; set; } = RequestStatus.Draft;
    public MatchDisposition? MatchDisposition { get; set; }
    public bool Open { get; set; } = true;


    public User? ResponseProcessUser { get; set; }
    public User? RequestProcessUser { get; set; }
    public EducationOrganization? EducationOrganization { get; set; }
    public List<PayloadContent>? PayloadContents { get; set; }
    public List<Message>? Messages { get; set; }
}