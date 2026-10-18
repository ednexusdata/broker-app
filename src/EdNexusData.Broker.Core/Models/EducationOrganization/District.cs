namespace EdNexusData.Broker.Core;

public class District
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string ShortName { get; set; } = default!;
    public string? Number { get; set; }

    public Address? Address { get; set; }

    public List<string> Domains { get; set; } = new();
    public string? Domain => Domains.FirstOrDefault();
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;

    public ICollection<School>? Schools { get; set; }

    public Common.EducationOrganizations.District ToCommon()
    {
        return new Common.EducationOrganizations.District()
        {
            Id = Id,
            Name = Name,
            ShortName = ShortName,
            Number = Number,
            Address = Address?.ToCommon(),
            Domains = Domains,
            TimeZone = TimeZone,
            Schools = Schools?.Select(s => s.ToCommon()).ToList()
        };
    }

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