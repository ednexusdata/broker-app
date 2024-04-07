using EdNexusData.Broker.Domain;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EdNexusData.Broker.Web.ViewModels.Users;

public class UserRequestViewModel
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


    public UserRequestViewModel() { }

    public UserRequestViewModel(
        User user,
        IdentityUser<Guid>? identityUser)
    {
        UserId = identityUser?.Id;
        LastName = user.LastName;
        FirstName = user.FirstName;
        Email = identityUser?.Email ?? string.Empty;
        IsSuperAdmin = user.IsSuperAdmin;
        AllEducationOrganizations = user.AllEducationOrganizations;
    }

    public UserRequestViewModel(
        IdentityUser<Guid>? identityUser,
        Guid? userId,
        string lastName,
        string firstName,
        string email,
        bool isSuperAdmin,
        PermissionType allEducationOrganizations)
    {
        IdentityUser = identityUser;
        UserId = userId;
        LastName = lastName;
        FirstName = firstName;
        Email = email;
        IsSuperAdmin = isSuperAdmin;
        AllEducationOrganizations = allEducationOrganizations;
    }
}
