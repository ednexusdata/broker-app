namespace EdNexusData.Broker.Web.Models;

public class FocusViewModel
{
    public string ReturnUrl { get; set; } = default!;
    
    // This is a string because it could be ALL or a Guid for an EducationOrganization.
    public string? FocusEducationOrganizationId { get; set; }
}