using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Core.Tests.Integration.Services;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Reports;
using EdNexusData.Broker.Data;
using Microsoft.EntityFrameworkCore;

namespace EdNexusData.Broker.Core.Tests.Integration.Fixtures;

public class BrokerWebDIServicesFixture : IDisposable
{
    private ServiceProvider? _serviceProvider;

    public ServiceProvider? Services
    {
        get
        {
            return _serviceProvider;
        }
    }

    public BrokerWebDIServicesFixture()
    {
        CreateServices();
        PrepareDbContext();

        BrokerDbFixture.Services = Services;

        Task.WaitAll(
            BrokerDbFixture.SeedDbContext()
        );
    }

    private void CreateServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddSingleton<IMemoryCache, MemoryCache>();

        services.AddDbContext<BrokerDbContext, PostgresDbContext>();
        services.AddScoped<DbContext, PostgresDbContext>();

        services.AddScoped(typeof(EfRepository<>));
        services.AddScoped(typeof(CachedRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(CachedRepository<>));

        services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = false;
        })
        .AddEntityFrameworkStores<BrokerDbContext>();

        services.AddScoped<ICurrentUser, CurrentUserService>();

        services.AddReportingServices();
        services.AddScoped<ProofOfRequestReport>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private void PrepareDbContext()
    {
        if (Services is null) { return; }

        var dbContext = Services.GetService<DbContext>();

        if (dbContext is null) { return; }

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        // clean up the setup code, if required
    }
}
