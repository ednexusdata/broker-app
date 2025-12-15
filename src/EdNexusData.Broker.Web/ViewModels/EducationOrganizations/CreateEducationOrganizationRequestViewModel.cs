using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Common.EducationOrganizations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EdNexusData.Broker.Web.ViewModels.EducationOrganizations;

public class CreateEducationOrganizationRequestViewModel
{
    [Display(Name = "Education Organization ID")]
    public Guid? EducationOrganizationId { get; set; }

    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = default!;

    [Required]
    [Display(Name = "Short Name")]
    public string ShortName { get; set; } = default!;

    [Display(Name = "Other")]
    public string? Number { get; set; } = default!;

    [Display(Name = "NCES")]
    public string? NcesCode { get; set; } = default!;
    
    [Display(Name = "CEEB")]
    public string? CeebCode { get; set; } = default!;

    [Display(Name = "State")]
    public string? StateCode { get; set; } = default!;

    [Required]
    [Display(Name = "Type")]
    public EducationOrganizationType EducationOrganizationType { get; set; } = default!;

    [Display(Name = "Parent Organization")]
    public Guid? ParentOrganizationId { get; set; }

    public Guid? DistrictParentOrganizationId { get; set; }
    public Guid? RegionParentOrganizationId { get; set; }
    
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
    
    [Display(Name = "Time Zone")]
    public string? TimeZone { get; set; } = TimeZoneInfo.Local.Id;

    [Display(Name = "Name")]
    public string? ContactName { get; set; }

    [Display(Name = "Job Title")]
    public string? ContactJobTitle { get; set; }

    [Display(Name = "Phone")]
    public string? ContactPhone { get; set; }

    [Display(Name = "Email")]
    public string? ContacEmail { get; set; }

    public IEnumerable<SelectListItem> DistrictEducationOrganizations { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> RegionEducationOrganizations { get; set; } = Enumerable.Empty<SelectListItem>();

    [Display(Name = "States")]
    public IEnumerable<SelectListItem> States { get; set; } = Enumerable.Empty<SelectListItem>();
}
