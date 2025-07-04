using Microsoft.Extensions.Caching.Memory;
using EdNexusData.Broker.Data;
using EdNexusData.Broker.Worker;
using EdNexusData.Broker.Worker.Services;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Worker;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    // Define the folder containing your appsettings.json files
    var configFolder = System.Environment.GetEnvironmentVariable("SETTINGS_FOLDER") ?? "/app/settings";

    // Load all appsettings.json files from the folder
    if (Directory.Exists(configFolder))
    {
        // Load the base appsettings.json
        config.AddJsonFile(Path.Combine(configFolder, "appsettings.json"), optional: false, reloadOnChange: true);

        // Determine the environment (default to Production)
        var env = hostingContext.HostingEnvironment.EnvironmentName ?? "Production";

        // Load environment-specific appsettings file
        config.AddJsonFile(Path.Combine(configFolder, $"appsettings.{env}.json"), optional: true, reloadOnChange: true);

        // Load environment variables (overrides JSON files)
        config.AddEnvironmentVariables();
    }
});

builder.ConfigureServices((hostContext, services) =>
{
    var serviceProvider = services.BuildServiceProvider();
    var logger = serviceProvider.GetService<ILogger<ApplicationLogger>>();
    services.AddSingleton(typeof(ILogger), logger!);
    services.AddSingleton(typeof(EdNexusData.Broker.Core.Environment), typeof(WorkerEnvironment));
    
    switch (hostContext.Configuration["Broker:DatabaseProvider"])
    {
        case DbProviderType.MsSql:
            services.AddDbContext<BrokerDbContext, MsSqlDbContext>();
            services.AddScoped<DbContext, MsSqlDbContext>();
            break;

        case DbProviderType.PostgreSql:
            services.AddDbContext<BrokerDbContext, PostgresDbContext>();
            services.AddScoped<DbContext, PostgresDbContext>();
            break;
    }

    services.AddScoped(typeof(EfRepository<>));
    services.AddScoped(typeof(CachedRepository<>));

    services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    services.AddScoped(typeof(IReadRepository<>), typeof(CachedRepository<>));

    services.AddSingleton(typeof(IMemoryCache), typeof(MemoryCache));
    services.AddSingleton(typeof(JobStatusStore));

    if (hostContext.Configuration["Broker:WorkerUserStamp"] is not null)
    {
        var current = new CurrentUserService(hostContext.Configuration["Broker:WorkerUserStamp"]!);
        services.AddSingleton<ICurrentUser>(current);
    }
    else
    {
        services.AddSingleton<ICurrentUser, CurrentUserService>();
    }

    X509Certificate2 certificate;
    if (hostContext.Configuration["DataProtection:PfxCertPassword"] == "null")
    {
        certificate = new X509Certificate2(hostContext.Configuration["DataProtection:PfxCertPath"]!);
    }
    else
    {
        certificate = new X509Certificate2(hostContext.Configuration["DataProtection:PfxCertPath"]!, hostContext.Configuration["DataProtection:PfxCertPassword"]!);
    }
    
    services.AddDataProtection()
        .PersistKeysToDbContext<BrokerDbContext>()
        .ProtectKeysWithCertificate(certificate)
        .SetApplicationName("EdNexusData.Broker");
    
    services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<BrokerDbContext>()
    .AddTokenProvider<DataProtectorTokenProvider<IdentityUser<Guid>>>(TokenOptions.DefaultProvider);
    
    if (EdNexusData.Broker.Core.Environment.IsNonProductionToLocalEnvironment(hostContext.HostingEnvironment.EnvironmentName)
        || hostContext.Configuration["Broker:IgnoreCertificateValidationCheck"] == "true")
    {
        services.AddHttpClient("default").ConfigurePrimaryHttpMessageHandler(() => {
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
    else
    {
        services.AddHttpClient("default");
    }

    services.AddBrokerServicesForWorker(hostContext.Configuration);
    services.AddConnectorServicesToDefaultProvider();

    services.AddHostedService<Worker>();
});
builder.UseSystemd();

var host = builder.Build();
host.Run();
