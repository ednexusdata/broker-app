// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using EdNexusData.Broker.Data;
using MediatR;
using Autofac;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using EdNexusData.Broker.Web;
using EdNexusData.Broker.Web.Services;
using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using EdNexusData.Broker.Web.Extensions.Routes;
using EdNexusData.Broker.Web.Services.PayloadContents;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using Microsoft.Extensions.Caching.Memory;
using EdNexusData.Broker.Web.Exceptions;
using Microsoft.AspNetCore.DataProtection;
using EdNexusData.Broker.Core.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Cryptography.X509Certificates;
using Community.Microsoft.Extensions.Caching.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// Define the folder containing your appsettings.json files
var configFolder = System.Environment.GetEnvironmentVariable("SETTINGS_FOLDER") ?? "/app/settings";

// Load all appsettings.json files from the folder
if (Directory.Exists(configFolder))
{
    // Load the base configuration first
    builder.Configuration.AddJsonFile(Path.Combine(configFolder, "appsettings.json"), optional: true, reloadOnChange: true);

    // Check the environment (default to Production if not set)
    var env = builder.Environment.EnvironmentName ?? "Production";

    // Load environment-specific settings (if they exist)
    builder.Configuration.AddJsonFile(Path.Combine(configFolder, $"appsettings.{env}.json"), optional: true, reloadOnChange: true);

    // Optional: Load environment variables (overrides appsettings.json values)
    builder.Configuration.AddEnvironmentVariables();
}

//builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
// Add services to the container.
builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<ScopedHttpContext>();
builder.Services.AddMediatR(typeof(Program).Assembly);

switch (builder.Configuration["DatabaseProvider"])
{
    case DbProviderType.MsSql:
        builder.Services.AddDbContext<BrokerDbContext, MsSqlDbContext>();
        builder.Services.AddScoped<DbContext, MsSqlDbContext>();
        break;

    case DbProviderType.PostgreSql:
        builder.Services.AddDbContext<BrokerDbContext, PostgresDbContext>();
        builder.Services.AddScoped<DbContext, PostgresDbContext>();
        break;
}

Console.WriteLine("PfxCertPath: " + builder.Configuration["DataProtection:PfxCertPath"]);

X509Certificate2 certificate;
if (builder.Configuration["DataProtection:PfxCertPassword"] == "null")
{
    certificate = new X509Certificate2(builder.Configuration["DataProtection:PfxCertPath"]!);
}
else
{
    certificate = new X509Certificate2(builder.Configuration["DataProtection:PfxCertPath"]!, builder.Configuration["DataProtection:PfxCertPassword"]!);
}
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<BrokerDbContext>()
    .ProtectKeysWithCertificate(certificate)
    .SetApplicationName("EdNexusData.Broker");

builder.Services.AddScoped(typeof(EfRepository<>));
builder.Services.AddScoped(typeof(CachedRepository<>));

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(CachedRepository<>));

builder.Services.AddSingleton(typeof(IMemoryCache), typeof(MemoryCache));
builder.Services.AddScoped(typeof(IMediator), typeof(Mediator));

builder.Services.AddSingleton(typeof(JobStatusStore));

builder.Services.AddSingleton(typeof(EdNexusData.Broker.Core.Environment), typeof(WebEnvironment));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

foreach (var assembly in Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => string.Equals(t.Namespace, "EdNexusData.Broker.Web.Helpers", StringComparison.Ordinal)).ToArray())
{
    builder.Services.AddScoped(assembly, assembly);
}

builder.Services.AddBrokerDataServices();

builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<BrokerDbContext>()
//.AddDefaultTokenProviders()
.AddTokenProvider<AuthenticatorTokenProvider<IdentityUser<Guid>>>(TokenOptions.DefaultAuthenticatorProvider);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPayloadContentService, PayloadContentService>();
//builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddTransient<CustomCookieAuthenticationEvents>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/AccessDenied";
    options.Cookie.Name = "EdNexusData.Broker.Identity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    //options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.LoginPath = "/Login";
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
    options.EventsType = typeof(CustomCookieAuthenticationEvents);
});

switch (builder.Configuration["DatabaseProvider"])
{
    case DbProviderType.MsSql:
        builder.Services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("MsSqlConnection");
            options.SchemaName = "dbo";
            options.TableName = "DistributedCache";
        });
        break;
    case DbProviderType.PostgreSql:
        builder.Services.AddDistributedPostgreSqlCache(options =>
        {
            options.ConnectionString = builder.Configuration.GetConnectionString("PostgreSqlConnection")!;
            options.SchemaName = "public"; // Optional: defaults to "public"
            options.TableName = "DistributedCache";   // Optional: defaults to "Cache"
        });
        break;
}

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "EdNexusData.Broker.Session";
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    //options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.IsEssential = true;
});

if (builder.Configuration["Authentication:Google:ClientId"] is not null &&
    builder.Configuration["Authentication:Google:ClientId"] != "")
{
    builder.Services.AddAuthentication().AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });
}

if (builder.Configuration["Authentication:Microsoft:ClientId"] is not null &&
    builder.Configuration["Authentication:Microsoft:ClientId"] != "")
{
    builder.Services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
    {
        microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
        microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
    });
}
    

builder.Services.AddAuthorization(options => {
    options.AddPolicy("SuperAdmin",
      policy => policy.RequireClaim("SuperAdmin", "true")
    );
    options.AddPolicy("AllEducationOrganizations",
      policy => policy.RequireClaim("AllEducationOrganizations", PermissionType.Read.ToString(), PermissionType.Write.ToString())
    );
    options.AddPolicy("TransferRecords",
      policy => policy.RequireClaim("TransferRecords", "true")
    );
    options.AddPolicy(TransferIncomingRecords,
      policy => policy.RequireClaim(TransferIncomingRecords, "true")
    );
    options.AddPolicy(TransferOutgoingRecords,
      policy => policy.RequireClaim(TransferOutgoingRecords, "true")
    );

    // var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
    //     CookieAuthenticationDefaults.AuthenticationScheme
    //     // "Identity.Application",
    //     // "Identity.External",
    //     // GoogleDefaults.AuthenticationScheme
    // );
    // defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
    // options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
});

builder.Services.AddTransient<IClaimsTransformation, BrokerClaimsTransformation>();

builder.Services.AddExceptionHandler<ForceLogoutExceptionHandler>();

builder.Services.AddControllersWithViews();

if (builder.Environment.EnvironmentName is not null 
    && EdNexusData.Broker.Core.Environment.IsNonProductionToLocalEnvironment(builder.Environment.EnvironmentName))
{
    builder.Services.AddHttpClient("default").ConfigurePrimaryHttpMessageHandler(() => {
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
    builder.Services.AddHttpClient("default");
}

builder.Services.AddScoped<ICurrentUser, CurrentUserService>();

builder.Services.AddBrokerServices();
builder.Services.AddConnectorServicesToDefaultProvider();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = false;
});

builder.Services.AddHostedService<BrokerDbContextInitializationService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear(); // Allow external proxies
    options.KnownProxies.Clear();
});

// builder.Services.AddHttpLogging(options =>  
// {
//     options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
//     options.RequestHeaders.Add("X-Forwarded-For");
//     options.RequestHeaders.Add("X-Forwarded-Proto");
//     options.ResponseHeaders.Add("X-Forwarded-For");
//     options.ResponseHeaders.Add("X-Forwarded-Proto");
// });

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

//app.UseHttpLogging();
using (var service = app.Services.CreateAsyncScope())
{
    var seederService = service.ServiceProvider.GetRequiredService<SeederService>();
    await seederService!.Invoke();
}


// Noted this way because of 
// https://github.com/dotnet/aspnetcore/issues/51888
app.UseExceptionHandler(o => { });

app.Use(async (context, next) =>
{
    logger.LogDebug("Before Forwarded Headers Processed | Request Scheme: {0}", context.Request.Scheme);

    context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto);
    logger.LogDebug("Before Forwarded Headers Processed | X-Forwarded-Proto: {Proto}", forwardedProto.ToString());
    
    context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor);
    logger.LogDebug("Before Forwarded Headers Processed | X-Forwarded-For: {For}", forwardedFor.ToString());
    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseForwardedHeaders();
    app.UseHsts();
    //app.UseHttpsRedirection();

    // app.Use((context, next) =>
    // {
    //     context.Request.Scheme = "https";
    //     return next(context);
    // });
}
else
{
    app.UseForwardedHeaders();
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseHttpMethodOverride(new HttpMethodOverrideOptions()
{
    FormFieldName = "_METHOD"
});

app.UseRouting();

app.Use(async (context, next) =>
{
    logger.LogDebug("After Forwarded Headers Processed | Request Scheme: {0}", context.Request.Scheme);

    context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto);
    logger.LogDebug("After Forwarded Headers Processed | X-Forwarded-Proto: {Proto}", forwardedProto.ToString());
    
    context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor);
    logger.LogDebug("After Forwarded Headers Processed | X-Forwarded-For: {For}", forwardedFor.ToString());
    await next();
});

// app.UseCookiePolicy(new CookiePolicyOptions()
// {
//     HttpOnly = HttpOnlyPolicy.Always,
//     Secure = CookieSecurePolicy.Always,
//     MinimumSameSitePolicy = SameSiteMode.Lax
// });

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoutes("system/organizations", "EducationOrganizations");
app.MapControllerRoutes("incoming-requests", "Incoming");
app.MapControllerRoutes("outgoing-requests", "Outgoing");
app.MapControllerRoutes("requests", "Requests");
app.MapControllerRoutes("system/users", "Users");
app.MapControllerRoute("systemjobs", "system/jobs", new { controller = "Jobs", action = "SystemIndex" });
app.MapControllerRoute("store-connectors", "store/connectors", new { controller = "Connectors", action = "Index" });
app.MapControllerRoute("userjobs", "jobs", new { controller = "Jobs", action = "Index"});
app.MapControllerRoutes("roles", "UserRoles");
app.MapControllerRoutes("settings", "Settings");
app.MapControllerRoutes("login", "Login");
app.MapControllerRoutes("focus", "Focus");
app.MapControllerRoutes("profile", "Profile");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Start();

// var defaultServiceProvider = app.Services.GetRequiredService<IServiceProvider>();
// BrokerServiceProvider.AddConnectorDependencies(defaultServiceProvider);

// var server = app.Services.GetService<IServer>();
// var addressFeature = server?.Features.Get<IServerAddressesFeature>();
// if (addressFeature is not null)
// {
//     var environ = app.Services.GetService<EdNexusData.Broker.Core.Environment>();
//     foreach (var address in addressFeature.Addresses)
//     {
//         environ!.AddAddress(address);
//     }
// }

using (var scoped = app.Services.CreateScope())
{
    // Verify database connection is up
    var dbConnectionService = scoped.ServiceProvider.GetService<DbConnectionService>()!;
    await dbConnectionService.ThrowIfDatabaseConnectionNotUpAsync();
}

app.WaitForShutdown();
//app.Run();
