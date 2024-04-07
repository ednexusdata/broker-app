using System.Linq.Expressions;
using Ardalis.Specification;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Exceptions;
using EdNexusData.Broker.Web.Specifications;
using static EdNexusData.Broker.Web.Constants.Sessions.SessionKey;

namespace EdNexusData.Broker.Web.Helpers;

public class FocusHelper
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<User> _userRepo;
    private readonly ISession _session;

    public FocusHelper(
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<UserRole> userRoleRepo,
        IRepository<User> userRepo,
        IHttpContextAccessor httpContextAccessor)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _userRepo = userRepo;
        _userRoleRepo = userRoleRepo;
        _session = httpContextAccessor!.HttpContext?.Session!;
    }

    public async Task<IEnumerable<SelectListItem>> GetFocusableEducationOrganizationsSelectList()
    {
        var selectListItems = new List<SelectListItem>();

        // Get currently logged in user
        var currentUser = _session.GetObjectFromJson<User>("User.Current");
        var allEdOrgs = currentUser?.AllEducationOrganizations;

        if (currentUser?.Id == null)
        {
            throw new ForceLogoutException();
        }

        var organizations = await _educationOrganizationRepository.ListAsync();
        organizations = organizations.OrderBy(x => x.ParentOrganization?.Name).ThenBy(x => x.Name).ToList();

        if (allEdOrgs == PermissionType.Read || allEdOrgs == PermissionType.Write)
        { 
            selectListItems.Add(new SelectListItem() {
                    Text = "All",
                    Value = "ALL",
                    Selected = _session.GetString(FocusOrganizationKey) == "ALL"
                });
            
            foreach(var organization in organizations)
            {
                selectListItems.Add(new SelectListItem() {
                    Text = (organization.EducationOrganizationType == EducationOrganizationType.District) 
                        ? organization.Name 
                        : $"{organization.ParentOrganization?.Name} / {organization.Name}",
                    Value = organization.Id.ToString(),
                    Selected = _session.GetString(FocusOrganizationKey) == organization.Id.ToString()
                });
            }
        }
        else
        {
            var userRoleSpec = new UserWithUserRolesByUserSpec(currentUser.Id);
            var user = await _userRepo.FirstOrDefaultAsync(userRoleSpec);
            var userRoles = user.UserRoles;

            foreach(var userRole in userRoles.Where(role => role.EducationOrganization?.ParentOrganizationId is not null))
            {
                selectListItems.Add(new SelectListItem() {
                    Text = $"{userRole.EducationOrganization?.ParentOrganization?.Name} / {userRole.EducationOrganization?.Name}",
                    Value = userRole.EducationOrganizationId.ToString(),
                    Selected = _session.GetString(FocusOrganizationKey) == userRole.EducationOrganizationId.ToString()
                });
            }
        }

        selectListItems = selectListItems.OrderBy(x => x.Text).ToList();

        var focusOrganizationId = _session.GetString(FocusOrganizationKey);

        if (string.IsNullOrEmpty(focusOrganizationId) && selectListItems.Any())
        {
            var firstSelectListItem = selectListItems.First();
            var educationOrganizationId = firstSelectListItem?.Value;

            if (!string.IsNullOrEmpty(educationOrganizationId) && Guid.TryParse(educationOrganizationId, out var organizationIdGuid))
            {
                _session.SetString(FocusOrganizationKey, educationOrganizationId);

                Expression<Func<EducationOrganization, bool>> focusOrganizationExpression = request => request.Id == organizationIdGuid;

                var specification = new SearchableWithPaginationSpecification<EducationOrganization>.Builder(1, -1)
                    .WithSearchExpression(focusOrganizationExpression)
                    .WithIncludeEntities(builder => builder
                        .Include(educationOrganization => educationOrganization.ParentOrganization))
                    .Build();

                var educationOrganizations = await _educationOrganizationRepository.ListAsync(specification, CancellationToken.None);
                var organization = educationOrganizations.FirstOrDefault();

                if (organization != null)
                {
                    var parentOrganizationName = organization.ParentOrganization?.Name;
                    var organizationName = organization.Name;

                    if (!string.IsNullOrWhiteSpace(parentOrganizationName))
                        _session.SetString(FocusOrganizationDistrict, parentOrganizationName);

                    _session.SetString(FocusOrganizationSchool, organizationName);
                }
            }
        }

        var selectedValue = _session.GetString(FocusOrganizationKey);

        var selectedSelectList = new SelectList(
                selectListItems,
                "Value",
                "Text",
                selectedValue
            );

        return selectedSelectList;
    }

    public async Task SetInitialFocus()
    {
        var currentUser = _session.GetObjectFromJson<User>("User.Current");
        var allEdOrgs = currentUser?.AllEducationOrganizations;

        if (allEdOrgs == PermissionType.Read || allEdOrgs == PermissionType.Write)
        {
            _session.SetString(FocusOrganizationKey, "ALL");
        }
        else
        {
            var userWithRoles = await _userRepo.FirstOrDefaultAsync(new UserWithUserRolesByUserSpec(currentUser!.Id));
            var userRoles = userWithRoles!.UserRoles;

            UserRole? first = null;
            if (userRoles!.Where(role => role.EducationOrganization?.ParentOrganizationId is not null).Any())
            {
                first = userRoles!.Where(role => role.EducationOrganization?.ParentOrganizationId is not null).FirstOrDefault();
            }
            else
            {
                first = userRoles!.Where(role => role.EducationOrganizationId is not null).FirstOrDefault()!;
            }
            if (first?.EducationOrganizationId is not null)
            _session.SetString(FocusOrganizationKey, first!.EducationOrganization!.Id.ToString());
        }
    }

    public Guid? CurrentEdOrgFocus()
    {
        var currentEdOrgFocus = _session.GetString(FocusOrganizationKey);
        if (currentEdOrgFocus != "ALL")
        {

            if (Guid.TryParse(currentEdOrgFocus, out Guid currentEdOrgFocusGuid))
            {
                return currentEdOrgFocusGuid;
            }
        }
        return null;
    }

    public async Task<Guid?> CurrentDistrictEdOrgFocus()
    {
        var currentEdOrgFocus = CurrentEdOrgFocus();

        // Check if district
        if (currentEdOrgFocus.HasValue)
        {
            var edOrg = await _educationOrganizationRepository.GetByIdAsync(currentEdOrgFocus.Value);
            if (edOrg is not null && edOrg.EducationOrganizationType == EducationOrganizationType.District)
            {
                return currentEdOrgFocus;
            }
        }
        return null;
    }

    public async Task<List<EducationOrganization>> GetFocusedSchools()
    {
        if (IsEdOrgAllFocus())
        {
            return await _educationOrganizationRepository.ListAsync(new OrganizationByTypeSpec(EducationOrganizationType.School));
        }
        else if (await CurrentDistrictEdOrgFocus() is not null)
        {
            return await _educationOrganizationRepository.ListAsync(new OrganizationByParentSpec((await CurrentDistrictEdOrgFocus()).Value));
        }
        else
        {
            if (CurrentEdOrgFocus().HasValue)
            {
                return await _educationOrganizationRepository.ListAsync(new OrganizationByIdWithParentSpec(CurrentEdOrgFocus()!.Value));
            }
            else
            {
                throw new ForceLogoutException();
            }
        }
    }

    public bool IsEdOrgAllFocus()
    {
        var currentEdOrgFocus = _session.GetString(FocusOrganizationKey);
        if (currentEdOrgFocus == "ALL")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
