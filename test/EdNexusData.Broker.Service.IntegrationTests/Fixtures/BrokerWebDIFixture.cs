using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EdNexusData.Broker.Service.IntegrationTests.Services;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Data;

namespace EdNexusData.Broker.Service.IntegrationTests.Fixtures;

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

        services.AddBrokerDataContext(configuration);

        services.AddScoped<ICurrentUser, CurrentUserService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }

    private void PrepareDbContext()
    {   
        if (Services is null) { return; }
        
        var dbContext = Services.GetService<BrokerDbContext>();
        
        if (dbContext is null) { return; }

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        // clean up the setup code, if required
    }
}