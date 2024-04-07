using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using System.Data;
using System.Security.Claims;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;

namespace EdNexusData.Broker.Web;

public class BrokerClaimsTransformation : IClaimsTransformation
{
    private readonly IRepository<User> _userRepo;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly ILogger<BrokerClaimsTransformation> _logger;

    private User? _user;

    public BrokerClaimsTransformation(ILogger<BrokerClaimsTransformation> logger, UserManager<IdentityUser<Guid>> userManager, IRepository<User> userRepo)
    {
        _logger = logger;
        _userManager = userManager;
        _userRepo = userRepo;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Start claims
        List<Claim> claims = new List<Claim>();

        // Attempt to load user
        var currentUser = await GetCurrentUser(principal);

        if (_user is null) { return principal; }

        // Get user-specific settings
        var claimType = "SuperAdmin";
        if (currentUser is null) return principal;
         
        if (!principal.HasClaim(claim => claim.Type == claimType))
        {
            if (currentUser.IsSuperAdmin == true)
            {
                claims.Add(new Claim(SuperAdmin, "true"));
                claims.Add(new Claim(TransferIncomingRecords, "true"));
                claims.Add(new Claim(TransferOutgoingRecords, "true"));
            }
        }

        claimType = "AllEducationOrganizations";
        if (!principal.HasClaim(claim => claim.Type == claimType))
        {
            if (currentUser.AllEducationOrganizations != PermissionType.None)
            {
                claims.Add(new Claim(claimType, currentUser.AllEducationOrganizations.ToString()));
            }
        }

        var userRoles = currentUser?.UserRoles;
        if (userRoles is not null && userRoles.Any(
            role => role.Role.ToString() == "IncomingProcessor"
            || role.Role.ToString() == "Processor") 
        )
        {
            claims.Add(new Claim(TransferIncomingRecords, "true"));
        }

        if (userRoles is not null && userRoles.Any(
            role => role.Role.ToString() == "OutgoingProcessor"
            || role.Role.ToString() == "Processor")
        )
        {
            claims.Add(new Claim(TransferOutgoingRecords, "true"));
        }

        claimType = "TransferRecords";
        // Get userroles
        if (!principal.HasClaim(claim => claim.Type == claimType))
        {
            if (userRoles?.Count > 0 || currentUser?.AllEducationOrganizations != PermissionType.None)
            {
                claims.Add(new Claim(claimType, "true"));
            }
        }

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