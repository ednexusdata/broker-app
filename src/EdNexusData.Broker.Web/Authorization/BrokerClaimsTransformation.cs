using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Claims;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using EdNexusData.Broker.Web.Helpers;
using static EdNexusData.Broker.Web.Constants.Sessions.SessionKey;

namespace EdNexusData.Broker.Web.Authorization;

public class BrokerClaimsTransformation : IClaimsTransformation
{
    private readonly IReadRepository<User> _userRepo;
    private readonly IReadRepository<EducationOrganization> educationOrganizationRepository;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly ILogger<BrokerClaimsTransformation> _logger;

    private User? _user;

    public BrokerClaimsTransformation(
        ILogger<BrokerClaimsTransformation> logger, 
        UserManager<IdentityUser<Guid>> userManager, 
        IReadRepository<User> userRepo,
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _logger = logger;
        _userManager = userManager;
        _userRepo = userRepo;
        this.educationOrganizationRepository = educationOrganizationRepository;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Start claims
        List<Claim> claims = new List<Claim>();

        // Attempt to load user
        var currentUser = await GetCurrentUser(principal);
        if (_user is null) { return principal; }
        if (currentUser is null) return principal;

        if (httpContextAccessor.HttpContext is not null)
        {
            string? sessionValue = httpContextAccessor.HttpContext.Session.GetString(FocusOrganizationKey);

            Console.WriteLine("Session Value in Claims Transformation Handler: " + sessionValue);
        }

        // If super admin, allow all claims
        if (currentUser.IsSuperAdmin == true)
        {
            claims.Add(new Claim(SuperAdmin, "true"));
            claims.Add(new Claim(TransferIncomingRecords, "true"));
            claims.Add(new Claim(TransferOutgoingRecords, "true"));
            claims.Add(new Claim(SystemAdministrator, "true"));
            claims.Add(new Claim(AllEducationOrganizations, PermissionType.Write.ToString()));
        }

        // Get user-specific settings
        if (!principal.HasClaim(claim => claim.Type == AllEducationOrganizations))
        {
            if (currentUser.AllEducationOrganizations != PermissionType.None)
            {
                claims.Add(new Claim(AllEducationOrganizations, currentUser.AllEducationOrganizations.ToString()));
            }
        }

        // Loop through all user roles for user and focused org
        if (currentUser.UserRoles is not null)
        {
            var currentUserRolesToProcess = new List<UserRole?>();
            
            var currentEdOrgFocus = FocusHelper.CurrentEdOrgFocus(httpContextAccessor.HttpContext!.Session);

            // See if there's a user group at the focused org
            var currentUserRole = currentUser.UserRoles
                .FirstOrDefault(ur => ur?.EducationOrganizationId == currentEdOrgFocus);
            if (currentUserRole is not null)
                currentUserRolesToProcess.Add(currentUserRole);

            // See if there's a user group up the stack
            var focusedEdOrgs = await FocusHelper.GetParentEdOrgs(httpContextAccessor.HttpContext!.Session, educationOrganizationRepository);

            var foundUserRoles = currentUser.UserRoles
                .Where(ur => ur?.EducationOrganizationId != null 
                    && focusedEdOrgs.Any(edOrg => edOrg.Id == ur.EducationOrganizationId))
                .ToList();
            if (foundUserRoles.Any())
            {
                currentUserRolesToProcess.AddRange(foundUserRoles);
            }

            foreach(var userRole in currentUserRolesToProcess)  
            {
                switch (userRole?.Role)
                {
                    case Role.Processor:
                        //if (!principal.HasClaim(claim => claim.Type == TransferIncomingRecords))
                            claims.Add(new Claim(TransferIncomingRecords, "true"));
                        //if (!principal.HasClaim(claim => claim.Type == TransferOutgoingRecords))
                            claims.Add(new Claim(TransferOutgoingRecords, "true"));
                        break;
                    case Role.IncomingProcessor:
                        //if (!principal.HasClaim(claim => claim.Type == TransferIncomingRecords))
                            claims.Add(new Claim(TransferIncomingRecords, "true"));
                        break;
                    case Role.OutgoingProcessor:
                        //if (!principal.HasClaim(claim => claim.Type == TransferOutgoingRecords))
                            claims.Add(new Claim(TransferOutgoingRecords, "true"));
                        break;
                    case Role.SystemAdministrator:
                            claims.Add(new Claim(SystemAdministrator, "true"));
                        //if (!principal.HasClaim(claim => claim.Type == TransferIncomingRecords))
                            claims.Add(new Claim(TransferIncomingRecords, "true"));
                        //if (!principal.HasClaim(claim => claim.Type == TransferOutgoingRecords))
                            claims.Add(new Claim(TransferOutgoingRecords, "true"));
                        break;
                }
            }
        }

        // Append claims to ClaimIdentity and then to Principal
        if (claims.Count > 0)
        {
            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaims(claims);

            principal.AddIdentity(claimsIdentity);
        }
        
        return principal;
    }

    private async Task<User?> GetCurrentUser(ClaimsPrincipal principal)
    {
        if (_user is null)
        {
            _logger.LogInformation("Current user not loaded for claims processing. Loading user.");
            // Get logged in user
            var email = principal.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()!.Value!;
            var userIdentity = await _userManager.FindByEmailAsync(email);

            if (userIdentity is not null)
            {
                _user = await _userRepo.FirstOrDefaultAsync(new ReadOnlyUserSpec(userIdentity.Id));
            }
        }
        else
        {
            _logger.LogInformation("Current user loaded.");
        }
        return _user;
    }
}