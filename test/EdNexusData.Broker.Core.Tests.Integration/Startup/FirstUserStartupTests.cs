using EdNexusData.Broker.Core.Tests.Integration.Fixtures;
using EdNexusData.Broker.Core.Tests.Integration.Services;
using EdNexusData.Broker.Data;
using BrokerDbContextInitializationService = EdNexusData.Broker.Web.Services.BrokerDbContextInitializationService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Core.Tests.Integration.Startup;

/// <summary>
/// The Web app creates the first user from configuration at startup, under the same database lock as migrations,
/// so servers starting together against an empty database create exactly one first user.
/// </summary>
public class FirstUserStartupTests
{
    private const string FirstUserEmail = "first.user@example.com";

    [Fact]
    public async Task Servers_starting_together_create_exactly_one_first_user()
    {
        await using var database = await TemporaryDatabase.CreateAsync();
        var servers = Enumerable.Range(0, 4).Select(_ => CreateServer(database)).ToList();

        try
        {
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var runs = servers.Select(server => Task.Run(async () =>
            {
                await start.Task;
                await RunStartupAsync(server);
            })).ToList();

            start.SetResult();
            await Task.WhenAll(runs).WaitAsync(TimeSpan.FromMinutes(2));
        }
        finally
        {
            foreach (var server in servers) await server.DisposeAsync();
        }

        await AssertSingleFirstUserAsync(database);
    }

    [Fact]
    public async Task Restarting_does_not_create_another_first_user()
    {
        await using var database = await TemporaryDatabase.CreateAsync();

        await using (var first = CreateServer(database)) await RunStartupAsync(first);
        await using (var restarted = CreateServer(database)) await RunStartupAsync(restarted);

        await AssertSingleFirstUserAsync(database);
    }

    /// <summary>The startup sequence from Program.cs: migrations, then first-user creation, under the lock.</summary>
    private static async Task RunStartupAsync(ServiceProvider server)
    {
        await using var scope = server.CreateAsyncScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
        var initializationService = scope.ServiceProvider.GetRequiredService<BrokerDbContextInitializationService>();

        await migrationService.InvokeAsync(() => initializationService.InvokeAsync());
    }

    private static async Task AssertSingleFirstUserAsync(TemporaryDatabase database)
    {
        await using var context = new PostgresDbContext(CreateConfiguration(database));

        var identityUser = Assert.Single(await context.Users.ToListAsync());
        Assert.Equal(FirstUserEmail, identityUser.Email);

        var brokerUser = Assert.Single(await context.ApplicationUsers!.ToListAsync());
        Assert.Equal(identityUser.Id, brokerUser.Id);
        Assert.True(brokerUser.IsSuperAdmin);
    }

    /// <summary>One "server": its own container, so its own caches and database connections.</summary>
    private static ServiceProvider CreateServer(TemporaryDatabase database)
    {
        var configuration = CreateConfiguration(database);
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        services.AddSingleton(configuration);
        services.AddLogging();
        services.AddMemoryCache();

        services.AddDbContext<BrokerDbContext, PostgresDbContext>();
        services.AddScoped<DbContext, PostgresDbContext>();
        services.AddScoped(typeof(EfRepository<>));
        services.AddScoped(typeof(CachedRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(CachedRepository<>));
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options => options.User.RequireUniqueEmail = false)
            .AddEntityFrameworkStores<BrokerDbContext>();
        services.AddScoped<IUserStore<IdentityUser<Guid>>, SlowUserStore>();

        services.AddBrokerDataServices();
        services.AddScoped<BrokerDbContextInitializationService>();

        return services.BuildServiceProvider();
    }

    private static IConfiguration CreateConfiguration(TemporaryDatabase database) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:PostgreSqlConnection"] = database.ConnectionString,
                ["DatabaseMigration:AutoMigrate"] = "true",
                ["FirstUser:EmailAddress"] = FirstUserEmail,
                ["FirstUser:WithPassword"] = "No",
                ["FirstUser:FirstName"] = "First",
                ["FirstUser:LastName"] = "User",
            })
            .Build();

    /// <summary>
    /// Waits before saving a user, widening the gap between the "are there any users?" check and the insert.
    /// Without the startup lock, a second server reliably sees no users in that gap and tries to create the
    /// same user too, failing its startup on the unique user name index.
    /// The gap must be longer than the 2 seconds between a waiting server's attempts to take the lock.
    /// </summary>
    private sealed class SlowUserStore : UserStore<IdentityUser<Guid>, IdentityRole<Guid>, BrokerDbContext, Guid>
    {
        public SlowUserStore(BrokerDbContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
        {
        }

        public override async Task<IdentityResult> CreateAsync(IdentityUser<Guid> user, CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            return await base.CreateAsync(user, cancellationToken);
        }
    }
}
