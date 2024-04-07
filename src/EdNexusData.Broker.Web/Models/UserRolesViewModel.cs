using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Web.Models;

public class UserRolesViewModel
{
    [Required]
    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public List<UserRoleViewModel>? UserRoles { get; set; }

    public IEnumerable<SelectListItem>? EducationOrganizations { get; set; }
    
    [Required]
    public Guid? EducationOrganizationId { get; set; }
    
    [Required]
    public Role? Role { get; set; }
}

public class UserRoleViewModel
{
    public UserRole? UserRole { get; set; }
    public string? DisplayText { get; set; }
}