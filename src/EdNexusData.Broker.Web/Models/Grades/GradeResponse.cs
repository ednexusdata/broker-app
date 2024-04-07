using src.Extensions.Descriptors;

namespace src.Models.Grades;
public class GradeResponse
{
    public string? Id { get; set; }
    public string? GradeTypeDescriptor { get; set; }
    public string? GradeType => GradeTypeDescriptor.ExtractDescriptor();
    public GradingPeriodReference GradingPeriodReference { get; set; } = new();
    public StudentSectionAssociationReference StudentSectionAssociationReference { get; set; } = new();
    public DateTime CurrentGradeAsOfDate { get; set; }
    public bool CurrentGradeIndicator { get; set; }
    public string? DiagnosticStatement { get; set; }
    public LearningStandardGrade[] LearningStandardGrades { get; set; } = Array.Empty<LearningStandardGrade>();
    public string? LetterGradeEarned { get; set; }
    public double NumericGradeEarned { get; set; }
    public string? PerformanceBaseConversionDescriptor { get; set; }
    public string? Etag { get; set; }
}
public class GradingPeriodReference
{
    public string? GradingPeriodDescriptor { get; set; }
    public string? GradingPeriod => GradingPeriodDescriptor.ExtractDescriptor();
    public int PeriodSequence { get; set; }
    public int SchoolId { get; set; }
    public int SchoolYear { get; set; }
}

public class StudentSectionAssociationReference
{
    public DateTime BeginDate { get; set; }
    public string? LocalCourseCode { get; set; }
    public int SchoolId { get; set; }
    public int SchoolYear { get; set; }
    public string? SectionIdentifier { get; set; }
    public string? SessionName { get; set; }
    public string? StudentUniqueId { get; set; }
}

public class LearningStandardReference
{
    public string? LearningStandardId { get; set; }
}

public class LearningStandardGrade
{
    public string? PerformanceBaseConversionDescriptor { get; set; }
    public string? PerformanceBaseConversion => PerformanceBaseConversionDescriptor.ExtractDescriptor();
    public string? DiagnosticStatement { get; set; }
    public string? LetterGradeEarned { get; set; }
    public double NumericGradeEarned { get; set; }
    public LearningStandardReference LearningStandardReference { get; set; } = new();
}
