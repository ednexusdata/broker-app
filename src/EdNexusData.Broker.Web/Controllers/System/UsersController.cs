using Ardalis.Specification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EdNexusData.Broker.Data;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Models;
using EdNexusData.Broker.Web.Models.Paginations;
using EdNexusData.Broker.Web.Models.Users;
using EdNexusData.Broker.Web.Specifications;
using EdNexusData.Broker.Web.ViewModels.Users;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.Constants.Claims;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = CustomClaimType.SystemAdministrator)]
public class UsersController : AuthenticatedController<UsersController>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserRole> userRoleRepository;
    private readonly BrokerDbContext _brokerDbContext;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly FocusHelper focusHelper;
    private readonly EducationOrganizationHelper educationOrganizationHelper;

    public UsersController(
        IRepository<User> userRepository,
        IRepository<UserRole> userRoleRepository,
        BrokerDbContext brokerDbContext,
        UserManager<IdentityUser<Guid>> userManager,
        FocusHelper focusHelper,
        EducationOrganizationHelper educationOrganizationHelper)
    {
        _userRepository = userRepository;
        this.userRoleRepository = userRoleRepository;
        _brokerDbContext = brokerDbContext;
        _userManager = userManager;
        this.focusHelper = focusHelper;
        this.educationOrganizationHelper = educationOrganizationHelper;
    }

    public async Task<IActionResult> Index(
      UserRequestModel model,
      CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions();

        if (!focusHelper.IsEdOrgAllFocus()) {
            var focusedEdOrgs = await focusHelper.GetFocusedEdOrgs();
            searchExpressions.Add(
                u => u.UserRoles != null 
                && u.UserRoles.Any(
                    r => r.EducationOrganization != null 
                        && focusedEdOrgs.Contains(r.EducationOrganization)
                )
            );
        }

        var sortExpression = model.BuildSortExpression();
        var identityUsers = await _brokerDbContext.Users.ToListAsync(cancellationToken);

        var specification = new SearchableWithPaginationSpecification<User>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .Build();

        var totalItems = await _userRepository.CountAsync(
            specification,
            cancellationToken);

        var users = await _userRepository.ListAsync(
            specification,
            cancellationToken);

        var userViewModels = users
            .Select(user => new UserRequestViewModel(
                user,
                identityUsers.FirstOrDefault(identityUser => identityUser.Id == user.Id)));

        var result = new PaginatedViewModel<UserRequestViewModel>(
            userViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return View(result);
    }

    public async Task<IActionResult> Create()
    {
        var model = new CreateUserRequestViewModel()
        {
            EducationOrganizations = await focusHelper.GetFocusableEducationOrganizationsSelectList(true)
        };
        
        return View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequestViewModel data)
    {
        if (!ModelState.IsValid) { TempData[VoiceTone.Critical] = "User not created."; return View("Create"); }

        var identityUser = new IdentityUser<Guid> { UserName = data.Email, Email = data.Email }; 
        var result = await _userManager.CreateAsync(identityUser);
        if (!result.Succeeded)
        {
            return BadRequest("There was an error creating the user.");
        }
        var user = new User()
        {
            Id = identityUser.Id,
            FirstName = data.FirstName,
            LastName = data.LastName,
            IsSuperAdmin = data.IsSuperAdmin,
            AllEducationOrganizations = data.AllEducationOrganizations
        };

        await _userRepository.AddAsync(user);

        // Add intial role if specified
        if (data.IsSuperAdmin != true && data.InitialUserRole is not null)
        {
            var userRole = new UserRole()
            {
                UserId = user.Id,
                Role = data.InitialUserRole.Role,
                EducationOrganizationId = data.InitialUserRole.EducationOrganizationId
            };
            await userRoleRepository.AddAsync(userRole);
        }

        TempData[VoiceTone.Positive] = $"Created user {data.Email} ({user.Id}).";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(Guid Id)
    {
        var identityUser = await _brokerDbContext.Users.Where(x => x.Id == Id).FirstOrDefaultAsync();
        var applicationUser = await _userRepository.GetByIdAsync(Id);

        var createUserViewModel = new CreateUserRequestViewModel();

        if (applicationUser is not null && identityUser is not null)
        {
            createUserViewModel = new CreateUserRequestViewModel()
            {
                UserId = applicationUser.Id,
                IdentityUser = identityUser,
                FirstName = applicationUser.FirstName,
                LastName = applicationUser.LastName,
                IsSuperAdmin = applicationUser.IsSuperAdmin,
                AllEducationOrganizations = applicationUser.AllEducationOrganizations,
                Email = identityUser.Email!,
                PasswordSet = await _userManager.HasPasswordAsync(identityUser)
            };
        }

        return View(createUserViewModel);
    }

    [ValidateAntiForgeryToken]
    [HttpPatch]
    public async Task<IActionResult> Update(UserViewModel data)
    {
        if (data.UserId is null) { throw new ArgumentException("Missing user id for processing."); }
        
        // Get existing user
        var user = await _userManager.FindByIdAsync(data.UserId.ToString()!);

        if (user is null) { throw new ArgumentException("Not a valid user."); }

        if (!ModelState.IsValid) { TempData[VoiceTone.Critical] = "User not updated."; return View("Edit"); }

        if (data.Email != user.Email)
        {
            // Update email
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, data.Email);
            var result = await _userManager.ChangeEmailAsync(user, data.Email, token);

            // Update user
            var userUpdateResult = await _userManager.SetUserNameAsync(user, data.Email.ToLower());
        }

        // Prepare user object
        var appUser = new User()
        {
            Id = user.Id,
            FirstName = data.FirstName,
            LastName = data.LastName,
            IsSuperAdmin = data.IsSuperAdmin,
            AllEducationOrganizations = data.AllEducationOrganizations
        };

        await _userRepository.UpdateAsync(appUser);

        TempData[VoiceTone.Positive] = $"Updated user {data.Email} ({user.Id}).";

        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpPatch]
    public async Task<IActionResult> TogglePassword(Guid id)
    {
        var identityUser = await _userManager.FindByIdAsync(id.ToString());

        if (identityUser is null) { throw new ArgumentNullException("Could not find user for id." ); }

        if (await _userManager.HasPasswordAsync(identityUser) == false)
        {
            var generatedPassword = BrokerIdentityUser.GenerateRandomPassword();

            await _userManager.AddPasswordAsync(identityUser, generatedPassword);
            await _userManager.ResetAuthenticatorKeyAsync(identityUser);
            var secretKey = await _userManager.GetAuthenticatorKeyAsync(identityUser);
            await _userManager.SetTwoFactorEnabledAsync(identityUser, true);

            var url = $"otpauth://totp/broker:{identityUser.Email}?secret={secretKey}&issuer=ednexusdata";
            
            TempData[VoiceTone.Positive] = $"Set password to {generatedPassword} and TOTP auth key url to {url} for user {identityUser.Email} ({identityUser.Id}).";
        }
        else
        {
            await _userManager.RemovePasswordAsync(identityUser);
            await _userManager.SetTwoFactorEnabledAsync(identityUser, false);
            await _userManager.ResetAuthenticatorKeyAsync(identityUser);

            TempData[VoiceTone.Positive] = $"Removed password and TOTP from {identityUser.Email} ({identityUser.Id}).";
        }

        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id)
    {
        var identityUser = await _userManager.FindByIdAsync(id.ToString());

        if (identityUser is null) { throw new ArgumentNullException("Could not find user for id." ); }

        var applicationUser = await _userRepository.GetByIdAsync(id);

        if (applicationUser is null) { throw new ArgumentNullException("Could not find app user for id." ); }

        await _userRepository.DeleteAsync(applicationUser);
        await _userManager.DeleteAsync(identityUser);

        TempData[VoiceTone.Positive] = $"Deleted user {identityUser.Email} ({id}).";

        return RedirectToAction(nameof(Index));
    }

}
