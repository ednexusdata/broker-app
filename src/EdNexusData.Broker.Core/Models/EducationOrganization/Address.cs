namespace EdNexusData.Broker.Core;

public class Address
{
    public string? StreetNumberName { get; set; } = default!;
    public string? City { get; set; } = default!;
    public string? StateAbbreviation { get; set; } = default!;
    public string? PostalCode { get; set; } = default!;

    public Common.EducationOrganizations.Address ToCommon()
    {
        return new Common.EducationOrganizations.Address()
        {
            StreetNumberName = StreetNumberName,
            City = City,
            StateAbbreviation = StateAbbreviation,
            PostalCode = PostalCode
        };
    }
}
