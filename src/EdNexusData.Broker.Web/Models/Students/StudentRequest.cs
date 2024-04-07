namespace src.Models.Students;
public record StudentRequest(
    string? FirstName,
    string? LastSurname,
    string? BirthCountryDescriptor,
    string? BirthSexDescriptor,
    string? BirthStateAbbreviationDescriptor,
    string? BirthCity,
    string? BirthDate,
    string? MiddleName,
    string? MaidenName,
    int? StudentUniqueId,
    int OffSet = 0,
    int Limit = 5);
