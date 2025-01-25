// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using EdNexusData.Broker.Common.EducationOrganizations;

namespace EdNexusData.Broker.Core;

public class EducationOrganization : BaseEntity, IAggregateRoot
{
    public EducationOrganization? ParentOrganization { get; set; }
    public Guid? ParentOrganizationId { get; set; }
    public string Name { get; set; } = default!;
    public string ShortName { get; set; } = default!;
    public string? Number { get; set; } = default!;
    public EducationOrganizationType EducationOrganizationType { get; set; } = EducationOrganizationType.District;
    public Address? Address { get; set; }
    public string? Domain { get; set; }
    public string? TimeZone { get; set; }
    public List<EducationOrganizationContact>? Contacts { get; set; }

    public virtual ICollection<EducationOrganization>? EducationOrganizations { get; set; }

    public Common.EducationOrganizations.EducationOrganization ToCommon()
    {
        var educationOrganization = new Common.EducationOrganizations.EducationOrganization()
        {
            Id = Id,
            ParentOrganizationId = ParentOrganizationId,
            Name = Name,
            ShortName = ShortName,
            Number = Number,
            EducationOrganizationType = EducationOrganizationType
        };

        return educationOrganization;
    }
}
