// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using static EdNexusData.Broker.Web.Constants.Sessions.SessionKey;
using EdNexusData.Broker.Web.Helpers;
using System.Security.Claims;
using EdNexusData.Broker.Service.Resolvers;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using System.Collections.Immutable;

namespace EdNexusData.Broker.Web.Controllers;

[AllowAnonymous]
public class LoginController : AuthenticatedController<LoginController>
{
    private readonly ILogger<LoginController> _logger;
    public readonly BrokerDbContext _db;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;
    private readonly IRepository<User> _userRepo;
    private readonly FocusHelper _focusHelper;
    private readonly AuthenticationProviderResolver _authenticationProviderResolver;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _hostingEnvironment;

    private ImmutableList<string> _allowedAnonymousEnvironments => new List<string> { "Demo", "Development", "Test" }.ToImmutableList();

    public LoginController(
        ILogger<LoginController> logger,
        BrokerDbContext db,
        UserManager<IdentityUser<Guid>> userManager,
        SignInManager<IdentityUser<Guid>> signInManager,
        IRepository<User> userRepo,
        FocusHelper focusHelper,
        AuthenticationProviderResolver authenticationProviderResolver,
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
    {
        _logger = logger;
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepo = userRepo;
        _focusHelper = focusHelper;
        _authenticationProviderResolver = authenticationProviderResolver;
        _configuration = configuration;
        _hostingEnvironment = hostingEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var loginViewModel = new LogInViewModel()
        {
            ExternalLogins = await _signInManager.GetExternalAuthenticationSchemesAsync()
        };

        return View(loginViewModel);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LogInViewModel loginViewModel)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(loginViewModel.Email!);

            if (user is null)
            {
                _logger.LogInformation("{Email} not found in database.", loginViewModel);
                return RedirectToAction("Index");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginViewModel.Password!, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // User successfully signed in with username and password.
                // Now verify the TOTP code.
                if (user != null && await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, loginViewModel.TotpCode!))
                {
                    // TOTP code is valid.
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

                    return LocalRedirect("~/");
                }
            }
        }

        _logger.LogInformation("{Email} not found in database.", loginViewModel);
        TempData[VoiceTone.Critical] = $"Invalid email/password/TOTP code combination.";
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    [Route("login/anonymous")]
    public async Task<IActionResult> AnonymousLogin(string email)
    {
        if (_configuration["Authentication:Anonymous"] is null
           || _configuration["Authentication:Anonymous"] != "Yes"
           || !_allowedAnonymousEnvironments.Contains(_hostingEnvironment.EnvironmentName))
        {
            return NotFound();
        }
        
        Guard.Against.Null(email, "email", $"Missing email in force login");

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            _logger.LogInformation("{Email} not found in database.", email);
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

        return LocalRedirect("~/");
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

            if (user is null)
            {
                await ProcessLogout();
                TempData[VoiceTone.Critical] = $"User {email} not found.";
                _logger.LogInformation($"User {email} not found in AspNetUsers.");
                return RedirectToAction("Index");
            }

            var currentUser = await _userRepo.GetByIdAsync(user.Id);
            
            if (currentUser is null)
            {
                await ProcessLogout();
                TempData[VoiceTone.Critical] = $"User {email} not found.";
                _logger.LogInformation($"User {email} not found in Users.");
                return RedirectToAction("Index");
            }

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
        await ProcessLogout();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index");
    }

    private async Task ProcessLogout()
    {
        await _signInManager.SignOutAsync();
        HttpContext?.Session.Clear();
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

        [Required]
        public string? TotpCode { get; set; }

        public string? ReturnUrl { get; set; }

        public IEnumerable<AuthenticationScheme>? ExternalLogins { get; set; }
    }
