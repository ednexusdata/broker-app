
using src.Extensions.Descriptors;

namespace src.Models.SectionAssociations;
public class SectionAssociationResponse
{
    public string? Id { get; set; }
    public DateTime BeginDate { get; set; }
    public SectionReference SectionReference { get; set; } = new();
    public string? AttemptStatusDescriptor { get; set; }
    public string? AttemptStatus => AttemptStatusDescriptor.ExtractDescriptor();
    public DateTime EndDate { get; set; }
    public bool HomeroomIndicator { get; set; }
    public string? RepeatIdentifierDescriptor { get; set; }
    public string? RepeatIdentifier => RepeatIdentifierDescriptor.ExtractDescriptor();
    public bool TeacherStudentDataLinkExclusion { get; set; }
    public string? Etag { get; set; }
}
public class SectionReference
{
    public string? LocalCourseCode { get; set; }
    public int SchoolId { get; set; }
    public int SchoolYear { get; set; }
    public string? SectionIdentifier { get; set; }
    public string? SessionName { get; set; }
}
