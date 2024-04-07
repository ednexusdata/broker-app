using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Web.ViewModels.EducationOrganizations;

public class EducationOrganizationRequestViewModel
{
    public Guid? Id { get; set; }

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

    [Display(Name = "Name")]
    public string? ContactName { get; set; }

    [Display(Name = "Job Title")]
    public string? ContactJobTitle { get; set; }

    [Display(Name = "Phone")]
    public string? ContactPhone { get; set; }

    [Display(Name = "Email")]
    public string? ContacEmail { get; set; }

    public EducationOrganization ParentOrganization { get; set; } = new();

    public EducationOrganizationRequestViewModel() { }

    public EducationOrganizationRequestViewModel(
        EducationOrganization educationOrganization)
    {
        Id = educationOrganization.Id;
        Name = educationOrganization.Name;
        Number = educationOrganization.Number ?? string.Empty;
        EducationOrganizationType = educationOrganization.EducationOrganizationType;
        ParentOrganizationId = educationOrganization.ParentOrganizationId;
        ParentOrganization = educationOrganization.ParentOrganization ?? new EducationOrganization();
        StreetNumberName = educationOrganization.Address?.StreetNumberName;
        City = educationOrganization.Address?.City;
        StateAbbreviation = educationOrganization.Address?.StateAbbreviation;
        PostalCode = educationOrganization.Address?.PostalCode;
        ContactName = educationOrganization.Contacts?.First().Name;
        ContactJobTitle = educationOrganization.Contacts?.First().JobTitle;
        ContacEmail = educationOrganization.Contacts?.First().Email;
        ContactPhone = educationOrganization.Contacts?.First().Phone;
    }

    public EducationOrganizationRequestViewModel(
        Guid? id,
        string name,
        string number,
        EducationOrganizationType educationOrganizationType,
        Guid? parentOrganizationId)
    {
        Id = id;
        Name = name;
        Number = number;
        EducationOrganizationType = educationOrganizationType;
        ParentOrganizationId = parentOrganizationId;
    }
}
