using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Domain;
using System.ComponentModel.DataAnnotations;
namespace EdNexusData.Broker.Web.ViewModels.OutgoingRequests;

public class CreateOutgoingRequestViewModel
{
    public Guid RequestId { get; set; }

    [Display(Name = "Student")]
    [Obsolete]
    public string? Student { get; set; } = string.Empty;

    [Display(Name = "Date")]
    public DateTimeOffset RequestReceived { get; set; }

    [Display(Name = "Student ID")]
    public string? StudentUniqueId { get; set; }

    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }

    [Display(Name = "Last Name")]
    public string? LastSurname { get; set; }

    [Display(Name = "Birth Date")]
    public string? BirthDate { get; set; }

    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    [Display(Name = "Grade")]
    public string? Grade { get; set; }
    public string? Additional { get; set; }

    [Display(Name = "Receiving Student")]
    public Student? ReceivingStudent { get; set; }

    [Display(Name = "Receiving District")]
    public District? ReceivingDistrict { get; set; }

    [Display(Name = "Receiving School")]
    public School? ReceivingSchool { get; set; }

    [Display(Name = "Receiving School Contact")]
    public EducationOrganizationContact? ReceivingSchoolContact { get; set; }

    [Display(Name = "Receiving Request Notes")]
    public string? ReceivingNotes { get; set; }

    [Display(Name = "Notes")]
    public string? ReleasingNotes { get; set; }

    [Display(Name = "Contents")]
    public List<string>? Contents { get; set; }

    [Display(Name = "Request Status")]
    // [Required(ErrorMessage = "Request Status is required")]
    public RequestStatus RequestStatus { get; set; } = RequestStatus.Draft;

    [Display(Name = "Files")]
    public IFormFileCollection Files { get; set; } = new FormFileCollection();
    public IEnumerable<SelectListItem> Genders { get; set; } = Enumerable.Empty<SelectListItem>();
    
    public List<PayloadContent>? ReceivingAttachments { get; set; }

    public List<PayloadContent>? DraftAttachments { get; set; }

    public List<PayloadContent>? ReleasingAttachments { get; set; }
}
