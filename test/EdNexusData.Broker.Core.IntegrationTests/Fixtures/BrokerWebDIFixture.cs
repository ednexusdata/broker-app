using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Core.IntegrationTests.Services;
using EdNexusData.Broker.Core;
using Microsoft.EntityFrameworkCore;

namespace EdNexusData.Broker.Core.IntegrationTests.Fixtures;

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
            .Build();

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        services.AddLogging();

        services.AddScoped<ICurrentUser, CurrentUserService>();
        
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