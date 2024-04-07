using src.Extensions.Descriptors;

namespace src.Models.Courses;
public class CourseTranscriptResponse
{
    public string? Id { get; set; }
    public CourseReference CourseReference { get; set; } = new();
    public StudentAcademicRecordReference StudentAcademicRecordReference { get; set; } = new();
    public string? CourseAttemptResultDescriptor { get; set; }
    public string CourseAttemptResult => CourseAttemptResultDescriptor.ExtractDescriptor();
    public string? AssigningOrganizationIdentificationCode { get; set; }
    public double AttemptedCredits { get; set; }
    public string? CourseCatalogURL { get; set; }
    public double EarnedCredits { get; set; }
    public string? FinalLetterGradeEarned { get; set; }
    public double FinalNumericGradeEarned { get; set; }
    public string? MethodCreditEarnedDescriptor { get; set; }
    public string MethodCreditEarned => MethodCreditEarnedDescriptor.ExtractDescriptor();
    public string? WhenTakenGradeLevelDescriptor { get; set; }
    public string WhenTakenGradeLevel => WhenTakenGradeLevelDescriptor.ExtractDescriptor();
    public List<AcademicSubject> AcademicSubjects { get; set; } = new();
    public string? Etag { get; set; }
    public DateTime LastModifiedDate { get; set; }
}

public class CourseReference
{
    public string? CourseCode { get; set; }
    public int EducationOrganizationId { get; set; }
}

public class StudentAcademicRecordReference
{
    public int EducationOrganizationId { get; set; }
    public int SchoolYear { get; set; }
    public string? StudentUniqueId { get; set; }
    public string? TermDescriptor { get; set; }
    public string Term => TermDescriptor.ExtractDescriptor();
}

public class AcademicSubject
{
    public string? AcademicSubjectDescriptor { get; set; }
    public string Subject => AcademicSubjectDescriptor.ExtractDescriptor();
}
