using src.Models.Courses;
using src.Models.ProgramAssociations;

namespace src.Models.Students;
public class RequestModel
{   
    public IEnumerable<ProgramAssociationResponse> ProgramAssociations { get; set;} = Enumerable.Empty<ProgramAssociationResponse>();
    public IEnumerable<CourseTranscriptResponse> CourseTranscripts { get; set;} = Enumerable.Empty<CourseTranscriptResponse>();
}
