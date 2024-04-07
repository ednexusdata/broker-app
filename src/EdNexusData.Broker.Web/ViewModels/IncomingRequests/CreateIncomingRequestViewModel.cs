using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Domain;
using src.Models.Courses;
using src.Models.ProgramAssociations;
using System.ComponentModel.DataAnnotations;

namespace EdNexusData.Broker.Web.ViewModels.IncomingRequests;

public class CreateIncomingRequestViewModel
{
    public Guid RequestId { get; set; }

    public List<EducationOrganization> EducationOrganizations { get; set; } = new List<EducationOrganization>();

    public List<SelectListItem> SelectEducationOrganizations
    {
        get {
            var educationOrganizationSelectList = new List<SelectListItem>();
            Guard.Against.Null(EducationOrganizations);
            foreach(var edOrgList in EducationOrganizations)
            {
                educationOrganizationSelectList.Add(new SelectListItem()
                {
                    Value = edOrgList.Id.ToString(),
                    Text = $"{edOrgList.ParentOrganization?.Name} / {edOrgList.Name}"
                });
            }
            educationOrganizationSelectList = educationOrganizationSelectList.OrderBy(x => x.Text).ToList();
            return educationOrganizationSelectList;
        }
    }

    [Display(Name = "Receiving School")]
    // [Required(ErrorMessage = "Education Organization is required")]
    public Guid? EducationOrganizationId { get; set; }

    [Display(Name = "Student")]
    // [Required(ErrorMessage = "Student is required")]
    public string? Student { get; set; } = string.Empty;

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

    [Display(Name = "District")]
    public District? ToDistrict { get; set; }

    [Display(Name = "School")]
    public School? ToSchool { get; set; }

    [Display(Name = "Notes")]
    public string? Note { get; set; }

    public string? Additional { get; set; }

    [Display(Name = "Contents")]
    public List<string>? Contents { get; set; }

    [Display(Name = "Request Status")]
    // [Required(ErrorMessage = "Request Status is required")]
    public RequestStatus RequestStatus { get; set; }

    public List<RequestStatus> RequestStatuses => 
        RequestStatus != RequestStatus.Imported 
            ? new List<RequestStatus> { RequestStatus.Draft, RequestStatus.Sent } 
            : new List<RequestStatus> { RequestStatus.Received, RequestStatus.Declined };

    [Display(Name = "Files")]
    public IFormFileCollection Files { get; set; } = new FormFileCollection();

    public IEnumerable<SelectListItem> States { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Genders { get; set; } = Enumerable.Empty<SelectListItem>();

    public List<PayloadContent>? ReceivingAttachments { get; set; }

    public List<PayloadContent>? DraftAttachments { get; set; }

    public List<PayloadContent>? ReleasingAttachments { get; set; }
}
