using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.Models;
using Ardalis.GuardClauses;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public class UserRolesController : AuthenticatedController<UserRolesController>
{
    private readonly IRepository<UserRole> _userRoleRepo;

    private readonly IRepository<User> _userRepo;

    private readonly EducationOrganizationHelper _edOrgHelper;
    
    public UserRolesController(
        IRepository<UserRole> userRoleRepo, 
        IRepository<User> userRepo, 
        EducationOrganizationHelper edOrgHelper)
    {
        _userRoleRepo = userRoleRepo;
        _userRepo = userRepo;
        _edOrgHelper = edOrgHelper;
    }
    
    public async Task<IActionResult> Index(Guid? Id)
    {
        Guard.Against.Null(Id, "Id", "Missing Id in request");
        
        var user = await _userRepo.GetByIdAsync(Id.Value);

        if (user is null) { return NotFound(); }

        var userRoleSpec = new UserRolesByUserSpec(user.Id);
        var userRoles = await _userRoleRepo.ListAsync(userRoleSpec);

        var userRoleViewModels = new List<UserRoleViewModel>();

        var existingOrganizations = new List<EducationOrganization>();

        if (userRoles is not null)
        {
            foreach(var userRole in userRoles)
            {
                userRoleViewModels.Add(new UserRoleViewModel() {
                    UserRole = userRole,
                    DisplayText = (userRole.EducationOrganization?.EducationOrganizationType == EducationOrganizationType.District) 
                        ? userRole.EducationOrganization?.Name 
                        : $"{userRole.EducationOrganization?.ParentOrganization?.Name} / {userRole.EducationOrganization?.Name}"
                }
                );

                existingOrganizations.Add(userRole.EducationOrganization!);
            }

            userRoleViewModels = userRoleViewModels.OrderBy(x => x.DisplayText).ToList();
        }

        var userRolesViewModel = new UserRolesViewModel() {
            UserId = user.Id,
            User = user,
            UserRoles = userRoleViewModels,
            EducationOrganizations = await _edOrgHelper.GetOrganizationsSelectList(existingOrganizations)
        };

        return View(userRolesViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserRolesViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData[VoiceTone.Critical] = "Missing organization, role, or user."; return View("Index", model);
        }
        
        var userRole = new UserRole()
        {
            Id = Guid.NewGuid(),
            UserId = model.UserId,
            EducationOrganizationId = model.EducationOrganizationId,
            Role = model.Role!.Value
        };

        await _userRoleRepo.AddAsync(userRole);

        TempData[VoiceTone.Positive] = $"Added user role. ({userRole.Id}).";

        return RedirectToAction("Index", new { Id = model.UserId });
    }

    [ValidateAntiForgeryToken]
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid? Id)
    {
        Guard.Against.Null(Id, "Id", "Missing Id in request");
        
        var organizationRole = await _userRoleRepo.GetByIdAsync(Id.Value);

        if (organizationRole is null) { throw new ArgumentException("Not a valid organization role."); }

        await _userRoleRepo.DeleteAsync(organizationRole);

        TempData[VoiceTone.Positive] = $"Deleted organization role ({organizationRole.Id}).";

        return RedirectToAction("Index", new { Id = organizationRole.UserId });
    }
}
