using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Web.ViewModels.EducationOrganizations;

public class CreateEducationOrganizationRequestViewModel
{
    [Display(Name = "Education Organization ID")]
    public Guid? EducationOrganizationId { get; set; }

    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = default!;

    [Required]
    [Display(Name = "Number")]
    public string Number { get; set; } = default!;

    [Required]
    [Display(Name = "Type")]
    public EducationOrganizationType EducationOrganizationType { get; set; } = default!;

    [Display(Name = "District")]
    public Guid? ParentOrganizationId { get; set; }

    [Display(Name = "Address")]
    public string? StreetNumberName { get; set; }

    [Display(Name = "City")]
    public string? City { get; set; }

    [Display(Name = "State")]
    public string? StateAbbreviation { get; set; }

    [Display(Name = "Zip Code")]
    public string? PostalCode { get; set; }

    [Display(Name = "Domain")]
    public string? Domain { get; set; }

    [Display(Name = "Name")]
    public string? ContactName { get; set; }

    [Display(Name = "Job Title")]
    public string? ContactJobTitle { get; set; }

    [Display(Name = "Phone")]
    public string? ContactPhone { get; set; }

    [Display(Name = "Email")]
    public string? ContacEmail { get; set; }

    [Display(Name = "Education Organizations")]
    public IEnumerable<SelectListItem> EducationOrganizations { get; set; } = Enumerable.Empty<SelectListItem>();

    [Display(Name = "States")]
    public IEnumerable<SelectListItem> States { get; set; } = Enumerable.Empty<SelectListItem>();
}
