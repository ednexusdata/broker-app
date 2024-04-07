using src.Models.Assessments;
using src.Models.Courses;
using src.Models.Grades;
using src.Models.ProgramAssociations;
using src.Models.SectionAssociations;

namespace src.Models.Students;
public class StudentAggregateResponse
{   
    public IEnumerable<AssessmentResponse> Assessments { get; set;} = Enumerable.Empty<AssessmentResponse>();
    public IEnumerable<ProgramAssociationResponse> ProgramAssociations { get; set;} = Enumerable.Empty<ProgramAssociationResponse>();
    public IEnumerable<SectionAssociationResponse> SectionAssociations { get; set;}= Enumerable.Empty<SectionAssociationResponse>();
    public IEnumerable<CourseTranscriptResponse> CourseTranscripts { get; set;} = Enumerable.Empty<CourseTranscriptResponse>();
    public IEnumerable<GradeResponse> Grades { get; set;} = Enumerable.Empty<GradeResponse>();
}
