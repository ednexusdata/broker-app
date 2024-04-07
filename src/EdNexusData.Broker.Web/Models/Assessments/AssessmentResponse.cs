using src.Extensions.Descriptors;

namespace src.Models.Assessments;
public class AssessmentResponse
{
    public string? Id { get; set; }
    public string? StudentAssessmentIdentifier { get; set; }
    public AssessmentReference AssessmentReference { get; set; } = new();
    public SchoolYearTypeReference SchoolYearTypeReference { get; set; }= new();
    public Accommodation[] Accommodations { get; set; } = Array.Empty<Accommodation>();
    public DateTime AdministrationDate { get; set; }
    public DateTime AdministrationEndDate { get; set; }
    public string? AdministrationEnvironmentDescriptor { get; set; }
    public string? AdministrationEnvironment => AdministrationEnvironmentDescriptor.ExtractDescriptor();
    public string? AdministrationLanguageDescriptor { get; set; }
    public string? AdministrationLanguage => AdministrationLanguageDescriptor.ExtractDescriptor();
    public int AssessedMinutes { get; set; }
    public string? EventCircumstanceDescriptor { get; set; }
    public string? EventDescription { get; set; }
    public Item[] Items { get; set; } = Array.Empty<Item>();
    public PerformanceLevel[] PerformanceLevels { get; set; }= Array.Empty<PerformanceLevel>();
    public Period Period { get; set; } = new();
    public string? PlatformTypeDescriptor { get; set; }
    public string? PlatformType => PlatformTypeDescriptor.ExtractDescriptor();
    public string? ReasonNotTestedDescriptor { get; set; }
    public string? ReasonNotTested => ReasonNotTestedDescriptor.ExtractDescriptor();
    public string? RetestIndicatorDescriptor { get; set; }
    public string? RetestIndicator => RetestIndicatorDescriptor.ExtractDescriptor();
    public ScoreResult2[] ScoreResults { get; set; }= Array.Empty<ScoreResult2>();
    public string? SerialNumber { get; set; }
    public StudentObjectiveAssessment[] StudentObjectiveAssessments { get; set; }= Array.Empty<StudentObjectiveAssessment>();
    public string? WhenAssessedGradeLevelDescriptor { get; set; }
    public string? WhenAssessedGradeLevel => WhenAssessedGradeLevelDescriptor.ExtractDescriptor();
    public string? Etag { get; set; }
}

public class AssessmentReference
{
    public string? AssessmentIdentifier { get; set; }
    public string? Namespace { get; set; }
}

public class SchoolYearTypeReference
{
    public int SchoolYear { get; set; }
}

public class Accommodation
{
    public string? AccommodationDescriptor { get; set; }
    public string? Name => AccommodationDescriptor.ExtractDescriptor();
}

public class AssessmentItemReference
{
    public string? AssessmentIdentifier { get; set; }
    public string? IdentificationCode { get; set; }
    public string? Namespace { get; set; }
}

public class Item
{
    public string? AssessmentItemResultDescriptor { get; set; }
    public string? AssessmentItemResult => AssessmentItemResultDescriptor.ExtractDescriptor();
    public string? ResponseIndicatorDescriptor { get; set; }
    public string? ResponseIndicator => ResponseIndicatorDescriptor.ExtractDescriptor();
    public string? AssessmentResponse { get; set; }
    public string? DescriptiveFeedback { get; set; }
    public int ItemNumber { get; set; }
    public int RawScoreResult { get; set; }
    public string? TimeAssessed { get; set; }
    public AssessmentItemReference AssessmentItemReference { get; set; } = new();
}

public class PerformanceLevel
{
    public string? AssessmentReportingMethodDescriptor { get; set; }
    public string? AssessmentReportingMethod=> AssessmentReportingMethodDescriptor.ExtractDescriptor();
    public string? PerformanceLevelDescriptor { get; set; }
    public string? PerformanceLevelName => PerformanceLevelDescriptor.ExtractDescriptor();
    public string? PerformanceLevelIndicatorName { get; set; }
}

public class Period
{
    public string? AssessmentPeriodDescriptor { get; set; }
    public string? AssessmentPeriod=> AssessmentPeriodDescriptor.ExtractDescriptor();
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ScoreResult
{
    public string? AssessmentReportingMethodDescriptor { get; set; }
    public string? AssessmentReportingMethod=> AssessmentReportingMethodDescriptor.ExtractDescriptor();
    public string? ResultDatatypeTypeDescriptor { get; set; }
    public string? ResultDatatypeType=> ResultDatatypeTypeDescriptor.ExtractDescriptor();
    public string? Result { get; set; }
}

public class ObjectiveAssessmentReference
{
    public string? AssessmentIdentifier { get; set; }
    public string? IdentificationCode { get; set; }
    public string? Namespace { get; set; }
}

public class StudentObjectiveAssessment
{
    public DateTime AdministrationDate { get; set; }
    public DateTime AdministrationEndDate { get; set; }
    public int AssessedMinutes { get; set; }
    public ObjectiveAssessmentReference ObjectiveAssessmentReference { get; set; } = new();
    public PerformanceLevel[] PerformanceLevels { get; set; } = Array.Empty<PerformanceLevel>();
    public ScoreResult[] ScoreResults { get; set; }= Array.Empty<ScoreResult>();
}

public class ScoreResult2
{
    public string? AssessmentReportingMethodDescriptor { get; set; }
    public string? AssessmentReportingMethod=> AssessmentReportingMethodDescriptor.ExtractDescriptor();
    public string? ResultDatatypeTypeDescriptor { get; set; }
    public string? ResultDatatypeType=> ResultDatatypeTypeDescriptor.ExtractDescriptor();
    public string? Result { get; set; }
}
