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
using EdNexusData.Broker.Core;
using Microsoft.AspNetCore.Authentication;
using EdNexusData.Broker.Web.Extensions.Routes;
using EdNexusData.Broker.Web.Services.PayloadContents;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using src.Services.Shared;
using Microsoft.Extensions.Caching.Memory;
using EdNexusData.Broker.Web.Exceptions;
using Microsoft.AspNetCore.DataProtection;
using EdNexusData.Broker.Core.Worker;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Autofac
//builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddDistributedMemoryCache();
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

builder.Services.AddDataProtection().PersistKeysToDbContext<BrokerDbContext>();

builder.Services.AddScoped(typeof(EfRepository<>));
builder.Services.AddScoped(typeof(CachedRepository<>));

builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(CachedRepository<>));

builder.Services.AddSingleton(typeof(IMemoryCache), typeof(MemoryCache));
builder.Services.AddScoped(typeof(IMediator), typeof(Mediator));

builder.Services.AddSingleton(typeof(JobStatusStore));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

foreach (var assembly in Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => String.Equals(t.Namespace, "EdNexusData.Broker.Web.Helpers", StringComparison.Ordinal)).ToArray())
{
    builder.Services.AddScoped(assembly, assembly);
}

builder.Services.AddBrokerDataServices();

builder.Services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
{
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<BrokerDbContext>();
//.AddDefaultTokenProviders();
//.AddTokenProvider<DataProtectorTokenProvider<IdentityUser<Guid>>>(TokenOptions.DefaultProvider);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPayloadContentService, PayloadContentService>();
//builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IClientService, ClientService>();

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

if (builder.Environment.IsDevelopment())
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
builder.Services.AddConnectorDependencies();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = false;
});

builder.Services.AddHostedService<BrokerDbContextInitializationService>();

var app = builder.Build();

using (var service = app.Services.CreateAsyncScope())
{
    var seederService = service.ServiceProvider.GetRequiredService<SeederService>();
    await seederService!.Invoke();
}

// Noted this way because of 
// https://github.com/dotnet/aspnetcore/issues/51888
app.UseExceptionHandler(o => { });

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseHttpMethodOverride(new HttpMethodOverrideOptions()
{
    FormFieldName = "_METHOD"
});

app.UseRouting();

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
app.MapControllerRoute("userjobs", "jobs", new { controller = "Jobs", action = "Index"});
app.MapControllerRoutes("roles", "UserRoles");
app.MapControllerRoutes("settings", "Settings");
app.MapControllerRoutes("login", "Login");
app.MapControllerRoutes("focus", "Focus");
app.MapControllerRoutes("profile", "Profile");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
