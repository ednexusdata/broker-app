// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
namespace EdNexusData.Broker.Core;

public class EducationOrganizationDomain : BaseEntity, IAggregateRoot
{
    public EducationOrganization? EducationOrganization { get; set; }
    public Guid EducationOrganizationId { get; set; }
    public string Domain { get; set; } = default!;
}
