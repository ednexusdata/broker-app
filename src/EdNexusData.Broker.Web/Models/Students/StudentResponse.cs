namespace src.Models.Students;
public record StudentResponse(
    string? Id,
    string? StudentUniqueId,
    DateTime? BirthDate,
    string? FirstName,
    string? LastSurname,
    string? PersonalTitlePrefix,
    DateTime LastModifiedDate);
