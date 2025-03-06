namespace EdNexusData.Broker.Core;

public class School
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string ShortName { get; set; } = default!;
    public string? Number { get; set; }

    public Address? Address { get; set; }
    public ICollection<EducationOrganizationContact>? Contacts { get; set; }

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