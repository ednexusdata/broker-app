using Ardalis.GuardClauses;
using EdNexusData.Broker.Data;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using Microsoft.AspNetCore.Identity;

namespace EdNexusData.Broker.Web.Services;

public class BrokerDbContextInitializationService : IHostedService
{
    private readonly ILogger<BrokerDbContextInitializationService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public BrokerDbContextInitializationService(
        ILogger<BrokerDbContextInitializationService> logger,
        IConfiguration configuration, IServiceProvider serviceProvider
    )
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scoped = _serviceProvider.CreateScope())
        {
            var _userRepository = (IReadRepository<User>)scoped.ServiceProvider.GetService(typeof(IReadRepository<User>))!;
            var _db = (BrokerDbContext)scoped.ServiceProvider.GetService(typeof(BrokerDbContext))!;
            var _userManager = (UserManager<IdentityUser<Guid>>)scoped.ServiceProvider.GetService(typeof(UserManager<IdentityUser<Guid>>))!;
            
            // Only run if there are no users
            var count = await _userRepository.CountAsync();
            _logger.LogInformation($"{count} users found in user database.");
            
            if (count == 0)
            {
                _logger.LogInformation("Finding first user in configuration.");
                var config = _configuration.GetSection("FirstUser");
                Guard.Against.Null(config, "config", "FirstUser not defined in configuration");

                _logger.LogInformation("Finding first user properties");

                var firstUserEmail = config.GetValue<string>("EmailAddress");
                Guard.Against.NullOrEmpty(firstUserEmail, "First User Email is null or empty");

                var firstUserPassword = config.GetValue<string>("WithPassword");
                Guard.Against.Null(firstUserPassword, "First User WithPassword is not defined");

                var firstUserFirstName = config.GetValue<string>("FirstName");
                Guard.Against.NullOrEmpty(firstUserFirstName, "First User First Name is null or empty");

                var firstUserLastName = config.GetValue<string>("LastName");
                Guard.Against.NullOrEmpty(firstUserLastName, "First User Last Name is null or empty");

                _logger.LogInformation("Creating first user in AspNet users");

                var identityUser = new IdentityUser<Guid> { UserName = firstUserEmail, Email = firstUserEmail }; 
                if (firstUserPassword == "Yes")
                {
                    var generatedPassword = BrokerIdentityUser.GenerateRandomPassword();
                    
                    await _userManager.CreateAsync(identityUser, generatedPassword);
                    await _userManager.ResetAuthenticatorKeyAsync(identityUser);
                    var secretKey = await _userManager.GetAuthenticatorKeyAsync(identityUser);
                    await _userManager.SetTwoFactorEnabledAsync(identityUser, true);

                    var url = $"otpauth://totp/broker:{firstUserEmail}?secret={secretKey}&issuer=broker";
                    _logger.LogInformation($"Set password for user: {generatedPassword}");
                    _logger.LogInformation($"Set totp on user: {url}");
                }
                else
                {
                    await _userManager.CreateAsync(identityUser);
                }
                
                _logger.LogInformation("Creating first user in Broker users");

                var user = new User()
                {
                    Id = identityUser.Id,
                    FirstName = firstUserFirstName,
                    LastName = firstUserLastName,
                    IsSuperAdmin = true,
                    CreatedAt = DateTime.UtcNow,
                    AllEducationOrganizations = PermissionType.Write
                };
                _db.Add(user);

                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"First User {firstUserFirstName} {firstUserLastName} {firstUserEmail} {identityUser.Id} created");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}