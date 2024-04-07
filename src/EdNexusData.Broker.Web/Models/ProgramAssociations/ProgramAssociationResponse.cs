using src.Extensions.Descriptors;

namespace src.Models.ProgramAssociations;
public class ProgramAssociationResponse
{
    public string? Id { get; set; }
    public ProgramReference ProgramReference { get; set; }= new();
    public DateTime BeginDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool ServedOutsideOfRegularSession { get; set; }
    public string? Etag { get; set; }
    public DateTime LastModifiedDate { get; set; }
}

public class ProgramReference
{
    public int EducationOrganizationId { get; set; }
    public string? ProgramName { get; set; }
    public string? ProgramTypeDescriptor { get; set; }
    public string? ProgramType => ProgramTypeDescriptor.ExtractDescriptor();
}
