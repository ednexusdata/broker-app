using Microsoft.AspNetCore.Identity;
using EdNexusData.Broker.Core;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EdNexusData.Broker.Web.ViewModels.Users;

	public class CreateUserRequestViewModel
	{
    public IdentityUser<Guid>? IdentityUser { get; set; }

    public Guid? UserId { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = default!;

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = default!;

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = default!;

    [Required]
    [Display(Name = "Super Admin")]
    public bool IsSuperAdmin { get; set; } = false;

    [Required]
    [Display(Name = "All Organizations")]
    public PermissionType AllEducationOrganizations { get; set; } = PermissionType.None;

    public bool PasswordSet { get; set; } = false;

    public UserRole? InitialUserRole { get; set; }

    public IEnumerable<SelectListItem>? EducationOrganizations { get; set; }
}

