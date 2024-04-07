using Microsoft.Extensions.Caching.Memory;
using EdNexusData.Broker.Data;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Worker;
using EdNexusData.Broker.Worker.Services;
using EdNexusData.Broker.Service;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    switch (hostContext.Configuration["DatabaseProvider"])
    {
        case DbProviderType.MsSql:
            services.AddDbContext<BrokerDbContext, MsSqlDbContext>();
            break;

        case DbProviderType.PostgreSql:
            services.AddDbContext<BrokerDbContext, PostgresDbContext>();
            break;
    }

    services.AddScoped(typeof(EfRepository<>));
    services.AddScoped(typeof(CachedRepository<>));

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    services.AddScoped(typeof(IReadRepository<>), typeof(CachedRepository<>));

    services.AddSingleton(typeof(IMemoryCache), typeof(MemoryCache));

    if (hostContext.Configuration["WorkerUserStamp"] is not null)
    {
        var current = new CurrentUserService(hostContext.Configuration["WorkerUserStamp"]!);
        services.AddSingleton<ICurrentUser>(current);
    }
    else
    {
        services.AddSingleton<ICurrentUser, CurrentUserService>();
    }
    
    
    if (hostContext.HostingEnvironment.IsDevelopment())
    {
        services.AddHttpClient("IgnoreSSL").ConfigurePrimaryHttpMessageHandler(() => {
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                }
            };
            return httpClientHandler;
        });
    }

    services.AddBrokerServicesForWorker();

    // Add connectors
    services.AddConnectorLoader();
    services.AddConnectorDependencies();

    services.AddHostedService<Worker>();
});

var host = builder.Build();

host.Run();
