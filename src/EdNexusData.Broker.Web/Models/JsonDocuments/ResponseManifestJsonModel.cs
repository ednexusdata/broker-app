using src.Models.Courses;
using src.Models.ProgramAssociations;

namespace EdNexusData.Broker.Web.Models.JsonDocuments;
#nullable disable
public class ResponseManifestJsonModel
{
    public Guid RequestId { get; set; }
    public string ResponseType { get; set; }
    public StudentJsonModel Student { get; set; }
    public SchoolJsonModel From { get; set; }
    public SchoolJsonModel To { get; set; }
    public string Note { get; set; }
    public List<string> Contents { get; set; }
    public IEnumerable<ProgramAssociationResponse> ProgramAssociations { get; set;} = Enumerable.Empty<ProgramAssociationResponse>();
    public IEnumerable<CourseTranscriptResponse> CourseTranscripts { get; set;} = Enumerable.Empty<CourseTranscriptResponse>();
}

