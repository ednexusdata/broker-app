using System.Linq.Expressions;
using Ardalis.Specification;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Web.Exceptions;
using EdNexusData.Broker.Web.Specifications;
using static EdNexusData.Broker.Web.Constants.Sessions.SessionKey;
using EdNexusData.Broker.Common.EducationOrganizations;

namespace EdNexusData.Broker.Web.Helpers;

public class FocusHelper
{
    private readonly IReadRepository<Core.EducationOrganization> educationOrganizationReadRepository;
    private readonly IRepository<Core.EducationOrganization> educationOrganizationRepository;
    private readonly IRepository<UserRole> _userRoleRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly SessionHelper sessionHelper;
    private readonly ISession _session;

    public FocusHelper(
        IReadRepository<Core.EducationOrganization> educationOrganizationReadRepository,
        IRepository<Core.EducationOrganization> educationOrganizationRepository,
        IRepository<UserRole> userRoleRepo,
        IRepository<User> userRepo,
        IHttpContextAccessor httpContextAccessor,
        SessionHelper sessionHelper)
    {
        this.educationOrganizationReadRepository = educationOrganizationReadRepository;
        this.educationOrganizationRepository = educationOrganizationRepository;
        _userRepo = userRepo;
        this.httpContextAccessor = httpContextAccessor;
        this.sessionHelper = sessionHelper;
        _userRoleRepo = userRoleRepo;
        _session = httpContextAccessor!.HttpContext?.Session!;
    }

    public async Task<IEnumerable<SelectListItem>> GetFocusableEducationOrganizationsSelectList(bool removeAll = false)
    {
        var selectListItems = new List<SelectListItem>();

        // Get currently logged in user
        var currentSessionUser = _session.GetObjectFromJson<User>("User.Current");
        if (currentSessionUser is null || currentSessionUser?.Id == null)
        {
            throw new ForceLogoutException();
        }

        // Requery for full user object from repo
        var currentUser = await _userRepo.FirstOrDefaultAsync(new UserWithUserRolesByUserSpec(currentSessionUser!.Id));
        if (currentUser is null)
        {
            throw new ForceLogoutException();
        }
        

        // var organizations = await _educationOrganizationRepository.ListAsync();
        // organizations = organizations.OrderBy(x => x.ParentOrganization?.Name).ThenBy(x => x.Name).ToList();
        if (removeAll == false)
        {
            var allEdOrgs = currentUser?.AllEducationOrganizations;
            if (allEdOrgs == PermissionType.Read || allEdOrgs == PermissionType.Write)
            { 
                selectListItems.Add(new SelectListItem() {
                        Text = "All",
                        Value = "ALL",
                        Selected = _session.GetString(FocusOrganizationKey) == "ALL"
                    });
            }
        }

        // var userRoleSpec = new UserWithUserRolesByUserSpec(currentUser.Id);
        // var user = await _userRepo.FirstOrDefaultAsync(userRoleSpec);
        // var userRoles = user?.UserRoles;

        // _ = userRoles ?? throw new NullReferenceException("No user roles assigned");

        foreach(var edOrg in await GetFocusableEdOrgs(currentUser!))
        {
            selectListItems.Add(new SelectListItem() {
                Text = edOrg?.FullName,
                Value = edOrg?.Id.ToString(),
                Selected = _session.GetString(FocusOrganizationKey) == edOrg?.Id.ToString()
            });
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

                Expression<Func<Core.EducationOrganization, bool>> focusOrganizationExpression = request => request.Id == organizationIdGuid;

                var specification = new SearchableWithPaginationSpecification<Core.EducationOrganization>.Builder(1, -1)
                    .WithSearchExpression(focusOrganizationExpression)
                    .WithIncludeEntities(builder => builder
                        .Include(educationOrganization => educationOrganization.ParentOrganization))
                    .Build();

                var educationOrganizations = await educationOrganizationReadRepository.ListAsync(specification, CancellationToken.None);
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
            if (userRoles!.Any(role => role.EducationOrganization?.ParentOrganizationId is not null))
            {
                first = userRoles!.FirstOrDefault(role => role.EducationOrganization?.ParentOrganizationId is not null);
            }
            else
            {
                first = userRoles!.FirstOrDefault(role => role.EducationOrganizationId is not null)!;
            }
            if (first?.EducationOrganizationId is not null)
            _session.SetString(FocusOrganizationKey, first!.EducationOrganization!.Id.ToString());
        }
    }

    public Guid? CurrentEdOrgFocus()
    {
        return CurrentEdOrgFocus(_session);
    }

    public static Guid? CurrentEdOrgFocus(ISession session)
    {
        var currentEdOrgFocus = session.GetString(FocusOrganizationKey);
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
            var edOrg = await educationOrganizationReadRepository.GetByIdAsync(currentEdOrgFocus.Value);
            if (edOrg is not null && edOrg.EducationOrganizationType == EducationOrganizationType.District)
            {
                return currentEdOrgFocus;
            }
        }
        return null;
    }

    public async Task<List<Core.EducationOrganization>> GetFocusedSchools()
    {
        var focusedEdOrgs = await GetFocusedEdOrgs();

        var focusedSchools = focusedEdOrgs
            .Where(edOrg => edOrg.EducationOrganizationType == EducationOrganizationType.School)
            .ToList();
        
        
        return focusedSchools;
    }

    public bool IsEdOrgAllFocus()
    {
        return IsEdOrgAllFocus(_session);
    }

    public static bool IsEdOrgAllFocus(ISession session)
    {
        var currentEdOrgFocus = session.GetString(FocusOrganizationKey);
        if (currentEdOrgFocus == "ALL")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static async Task<List<Core.EducationOrganization>> GetParentEdOrgs(
        ISession session,
        IHttpContextAccessor httpContextAccessor,
        IReadRepository<Core.EducationOrganization> educationOrganizationRepository
    )
    {
        if (IsEdOrgAllFocus(session))
        {
            return await educationOrganizationRepository.ListAsync();
        }
        else if (CurrentEdOrgFocus(session) is not null)
        {
            var focusedEdOrgList = new List<Core.EducationOrganization>();
            var focusedEdOrg = await educationOrganizationRepository
                .FirstOrDefaultAsync(new OrganizationWithChildSpec(CurrentEdOrgFocus(session)!.Value));
            _ = focusedEdOrg ?? throw new NullReferenceException("Focused Education Organization not found");
            
            Func<Core.EducationOrganization, 
                 List<Core.EducationOrganization>, 
                 Task<List<Core.EducationOrganization>>> orgRecursion = null!;

            orgRecursion = async (focusedEdOrg, focusedEdOrgList) => 
            {
                // Make sure it's actually loaded
                focusedEdOrg = (await educationOrganizationRepository
                    .FirstOrDefaultAsync(new OrganizationByIdWithParentSpec(focusedEdOrg.Id)))!;
                
                if (focusedEdOrg.ParentOrganization != null)
                {
                    focusedEdOrgList.Add(focusedEdOrg.ParentOrganization);
                    await orgRecursion(focusedEdOrg.ParentOrganization, focusedEdOrgList);
                    return focusedEdOrgList;
                }
                else
                {
                   return focusedEdOrgList;
                }
            };

            return await orgRecursion(focusedEdOrg, focusedEdOrgList);
        }
        else
        {
            await SessionHelper.InvalidateUserSessionAsync(httpContextAccessor);
            return new List<Core.EducationOrganization>();
        }
    }

    public async Task<List<Core.EducationOrganization>> GetFocusedEdOrgs(bool ignoreCache = false)
    {
        if (IsEdOrgAllFocus())
        {
            if (ignoreCache == true)
            {
                return await educationOrganizationRepository.ListAsync();
            }

            return await educationOrganizationReadRepository.ListAsync();
        }
        else if (CurrentEdOrgFocus() is not null)
        {
            var focusedEdOrgList = new List<Core.EducationOrganization>();
            var focusedEdOrg = await educationOrganizationReadRepository
                .FirstOrDefaultAsync(new OrganizationWithChildSpec(CurrentEdOrgFocus()!.Value));
            _ = focusedEdOrg ?? throw new NullReferenceException("Focused Education Organization not found");
            
            Func<Core.EducationOrganization, 
                 List<Core.EducationOrganization>, 
                 Task<List<Core.EducationOrganization>>> orgRecursion = null!;

            orgRecursion = async (focusedEdOrg, focusedEdOrgList) => 
            {
                // Make sure it's actually loaded
                focusedEdOrg = (await educationOrganizationReadRepository
                    .FirstOrDefaultAsync(new OrganizationWithChildSpec(focusedEdOrg.Id)))!;
                
                if (focusedEdOrg.EducationOrganizations != null && focusedEdOrg.EducationOrganizations.Count > 0)
                {
                    focusedEdOrgList.Add(focusedEdOrg);
                    foreach (var childOrg in focusedEdOrg.EducationOrganizations)
                    {
                        await orgRecursion(childOrg, focusedEdOrgList);
                    }
                    return focusedEdOrgList;
                }
                else
                {
                   focusedEdOrgList.Add(focusedEdOrg);
                   return focusedEdOrgList;
                }
            };

            return await orgRecursion(focusedEdOrg, focusedEdOrgList);
        }
        else
        {
            if (CurrentEdOrgFocus().HasValue)
            {
                return await educationOrganizationReadRepository.ListAsync(new OrganizationByIdWithParentSpec(CurrentEdOrgFocus()!.Value));
            }
            else
            {
                await sessionHelper.InvalidateUserSessionAsync();
                return new List<Core.EducationOrganization>();
            }
        }
    }

    public static async Task<List<Core.EducationOrganization>> GetFocusedEdOrgs(
        ISession session, 
        IReadRepository<Core.EducationOrganization> educationOrganizationRepository)
    {
        if (IsEdOrgAllFocus(session))
        {
            return await educationOrganizationRepository.ListAsync();
        }
        else if (CurrentEdOrgFocus(session) is not null)
        {
            var focusedEdOrgList = new List<Core.EducationOrganization>();
            var focusedEdOrg = await educationOrganizationRepository
                .FirstOrDefaultAsync(new OrganizationWithChildSpec(CurrentEdOrgFocus(session)!.Value));
            _ = focusedEdOrg ?? throw new NullReferenceException("Focused Education Organization not found");
            
            Func<Core.EducationOrganization, 
                 List<Core.EducationOrganization>, 
                 Task<List<Core.EducationOrganization>>> orgRecursion = null!;

            orgRecursion = async (focusedEdOrg, focusedEdOrgList) => 
            {
                // Make sure it's actually loaded
                focusedEdOrg = (await educationOrganizationRepository
                    .FirstOrDefaultAsync(new OrganizationWithChildSpec(focusedEdOrg.Id)))!;
                
                if (focusedEdOrg.EducationOrganizations != null && focusedEdOrg.EducationOrganizations.Count > 0)
                {
                    focusedEdOrgList.Add(focusedEdOrg);
                    foreach (var childOrg in focusedEdOrg.EducationOrganizations)
                    {
                        await orgRecursion(childOrg, focusedEdOrgList);
                    }
                    return focusedEdOrgList;
                }
                else
                {
                   focusedEdOrgList.Add(focusedEdOrg);
                   return focusedEdOrgList;
                }
            };

            return await orgRecursion(focusedEdOrg, focusedEdOrgList);
        }
        else
        {
            if (CurrentEdOrgFocus(session).HasValue)
            {
                return await educationOrganizationRepository.ListAsync(new OrganizationByIdWithParentSpec(CurrentEdOrgFocus(session)!.Value));
            }
            else
            {
                throw new ForceLogoutException();
            }
        }
    }

    public async Task<List<Core.EducationOrganization>> GetFocusableEdOrgs(User currentUser)
    {
        if (currentUser.AllEducationOrganizations == PermissionType.Read || currentUser.AllEducationOrganizations == PermissionType.Write)
        {
            return await educationOrganizationReadRepository.ListAsync();
        }
        else
        {
            var focusableEdOrgList = new List<Core.EducationOrganization>();
            
            Func<Core.EducationOrganization, 
                 List<Core.EducationOrganization>, 
                 Task<List<Core.EducationOrganization>>> orgRecursion = null!;

            orgRecursion = async (focusedEdOrg, focusableEdOrgList) => 
            {
                // Make sure it's actually loaded
                focusedEdOrg = (await educationOrganizationReadRepository
                    .FirstOrDefaultAsync(new OrganizationWithChildSpec(focusedEdOrg.Id)))!;
                
                if (focusedEdOrg.EducationOrganizations != null && focusedEdOrg.EducationOrganizations.Count > 0)
                {
                    focusableEdOrgList.Add(focusedEdOrg);
                    foreach (var childOrg in focusedEdOrg.EducationOrganizations)
                    {
                        await orgRecursion(childOrg, focusableEdOrgList);
                    }
                    return focusableEdOrgList;
                }
                else
                {
                   focusableEdOrgList.Add(focusedEdOrg);
                   return focusableEdOrgList;
                }
            };

            // Get list of orgs in user roles
            var userRoles = currentUser.UserRoles;

            // Loop through each user role and add to org list
            if (currentUser.UserRoles is not null)
            foreach (var userRole in currentUser.UserRoles)
            {
                if (userRole.EducationOrganization is not null)
                    await orgRecursion(userRole.EducationOrganization, focusableEdOrgList);
            }

            return focusableEdOrgList;
        }
    }

}
