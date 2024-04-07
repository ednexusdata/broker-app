// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using static EdNexusData.Broker.Web.Constants.Sessions.SessionKey;
using EdNexusData.Broker.Web.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using EdNexusData.Broker.Service.Resolvers;
using Ardalis.GuardClauses;
using Microsoft.Identity.Web;
using EdNexusData.Broker.Domain.Specifications;

namespace EdNexusData.Broker.Web.Controllers;

[AllowAnonymous]
public class LoginController : AuthenticatedController<LoginController>
{
    private readonly ILogger<LoginController> _logger;
    //private readonly EdNexusData.Broker.Connector.Edupoint.Synergy.Authentication.ThirdPartyApplication _auth;
    //private readonly AuthenticationProvidersLocator? _authProvidersLocator;
    public readonly BrokerDbContext _db;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly IRepository<User> _userRepo;
    private readonly FocusHelper _focusHelper;
    private readonly AuthenticationProviderResolver _authenticationProviderResolver;

    public LoginController(
        ILogger<LoginController> logger,
        BrokerDbContext db,
        UserManager<IdentityUser<Guid>> userManager,
        SignInManager<IdentityUser<Guid>> signInManager,
        IRepository<User> userRepo,
        FocusHelper focusHelper,
        AuthenticationProviderResolver authenticationProviderResolver)
    {
        _logger = logger;
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepo = userRepo;
        _focusHelper = focusHelper;
        _authenticationProviderResolver = authenticationProviderResolver;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var externalLogins = await _signInManager.GetExternalAuthenticationSchemesAsync();
        return View(externalLogins);
    }

    [HttpGet]
    public async Task<IActionResult> CreateFirstUser()
    {
        var identityUser = new IdentityUser<Guid> { UserName = "luis.ruiz@developers.net", Email = "luis.ruiz@developers.net" }; 
        var result = await _userManager.CreateAsync(identityUser);

        var user = new User()
        {
            Id = identityUser.Id,
            FirstName = "Luis",
            LastName = "Ruiz",
            IsSuperAdmin = true,
            CreatedAt = DateTime.UtcNow,
            AllEducationOrganizations = PermissionType.Write
        };
        _db.Add(user);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("login/externallogin")]
    public IActionResult ExternalLogin(string provider, string? returnUrl)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Action("ExternalLoginCallback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    [HttpGet]
    [Route("login/externallogin")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        if (remoteError != null)
        {
            //ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToAction("Index", new { ReturnUrl = returnUrl });
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            //ErrorMessage = "Error loading external login information.";
            return RedirectToAction("Index", new { ReturnUrl = returnUrl });
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            var email = info.Principal.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()!.Value!;
            var user = await _userManager.FindByEmailAsync(email);

            var currentUser = await _userRepo.GetByIdAsync(user!.Id);
            HttpContext?.Session?.SetObjectAsJson(UserCurrent, currentUser!);
            await _focusHelper.SetInitialFocus();
            
            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info!.Principal.Identity?.Name, info.LoginProvider);

            HttpContext?.Session?.SetString(LastAccessedKey, $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            return LocalRedirect(returnUrl);
        }
        if (result.IsLockedOut)
        {
            return RedirectToAction("Lockout");
        }
        else
        {
            // Get user
            var email = info.Principal.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()!.Value!;
            var user = await _userManager.FindByEmailAsync(email);

            var currentUser = await _userRepo.GetByIdAsync(user!.Id);
            HttpContext?.Session?.SetObjectAsJson(UserCurrent, currentUser!);
            await _focusHelper.SetInitialFocus();

            if (user is null)
            {
                _logger.LogInformation("{Email} not found in database.", email);
                return RedirectToAction("Index");
            }
            
            var loginResult = await _userManager.AddLoginAsync(user, info);
            if (loginResult.Succeeded)
            {
                _logger.LogInformation("Added {Name} logged in with {LoginProvider} provider.", info!.Principal.Identity?.Name, info.LoginProvider);
            }
            result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info!.Principal.Identity?.Name, info.LoginProvider);

                HttpContext?.Session?.SetString(LastAccessedKey, $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    [Route("login/connector/{provider}")]
    public async Task<IActionResult> ProviderLogin(string provider, string returnUrl)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        // Find provider
        var authenticationProvider = _authenticationProviderResolver.Resolve(provider);
        
        Guard.Against.Null(authenticationProvider, "authenticationProvider", $"Unable to find authentication provider for {provider}");

        var authUser = await authenticationProvider.AuthenticateAsync(HttpContext.Request);

        var user = await _userManager.FindByEmailAsync(authUser.Email);

        if (user is null)
        {
            _logger.LogInformation("{Email} not found in database.", authUser.Email);
            return RedirectToAction("Index");
        }

        var currentUser = await _userRepo.GetByIdAsync(user.Id);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email!),
            new Claim(ClaimTypes.Email, user.Email!)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, IdentityConstants.ApplicationScheme);

        await HttpContext!.SignInAsync(
            IdentityConstants.ApplicationScheme,
            new ClaimsPrincipal(claimsIdentity));
        
        HttpContext?.Session?.SetObjectAsJson(UserCurrent, currentUser!);
        await _focusHelper.SetInitialFocus();
        HttpContext?.Session?.SetString(LastAccessedKey, $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

        return LocalRedirect(returnUrl);
    }

    [Route("login/logout")]
    public async Task<IActionResult> Logout()
    {
         await _signInManager.SignOutAsync();
        HttpContext?.Session.Clear();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index");
    }

}

public class LogInViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }

        public IList<AuthenticationScheme>? ExternalLogins { get; set; }
    }
