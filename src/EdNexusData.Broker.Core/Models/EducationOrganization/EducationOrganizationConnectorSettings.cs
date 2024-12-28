// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Text.Json;

namespace EdNexusData.Broker.Core;

public class EducationOrganizationConnectorSettings : BaseEntity, IAggregateRoot
{
    public EducationOrganization? EducationOrganization { get; set; }
    public Guid? EducationOrganizationId { get; set; }
    public string Connector { get; set; } = default!;
    public JsonDocument? Settings { get; set; }
}
