namespace EdNexusData.Broker.Core;

public class District
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string ShortName { get; set; } = default!;
    public string? Number { get; set; }

    public Address? Address { get; set; }

    public string? Domain { get; set; }
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    
    public ICollection<School>? Schools { get; set; }

    public Common.EducationOrganizations.EducationOrganization ToCommonEducationOrganization()
    {
        return new Common.EducationOrganizations.EducationOrganization()
        {
            Id = Id,
            Name = Name,
            ShortName = ShortName,
            Number = Number
        };
    }
}