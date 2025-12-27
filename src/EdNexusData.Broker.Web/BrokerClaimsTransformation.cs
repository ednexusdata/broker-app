using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using EdNexusData.Broker.Core;
using System.Data;
using System.Security.Claims;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using EdNexusData.Broker.Core.Specifications;
using EdNexusData.Broker.Web.Helpers;

namespace EdNexusData.Broker.Web;

public class BrokerClaimsTransformation : IClaimsTransformation
{
    private readonly IReadRepository<User> _userRepo;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly ILogger<BrokerClaimsTransformation> _logger;

    private User? _user;

    public BrokerClaimsTransformation(
        ILogger<BrokerClaimsTransformation> logger, 
        UserManager<IdentityUser<Guid>> userManager, 
        IReadRepository<User> userRepo
    )
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
        if (currentUser is null) return principal;

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

        // Loop through all UserRoles for user
        if (currentUser.UserRoles is not null)
        {
            foreach(var userRole in currentUser.UserRoles)  
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