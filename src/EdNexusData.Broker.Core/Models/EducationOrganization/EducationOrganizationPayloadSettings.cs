// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
namespace EdNexusData.Broker.Core;

public class EducationOrganizationPayloadSettings : BaseEntity, IAggregateRoot
{
    public EducationOrganization? EducationOrganization { get; set; }
    public Guid? EducationOrganizationId { get; set; }
    public string Payload { get; set; } = default!;
    public IncomingPayloadSettings? IncomingPayloadSettings { get; set; }
    public OutgoingPayloadSettings? OutgoingPayloadSettings { get; set; }
}
