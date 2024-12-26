using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EdNexusData.Broker.Data;
using EdNexusData.Broker.Domain;

namespace Microsoft.Extensions.DependencyInjection;

public static class BrokerDataServiceCollectionExtension
{
    /*
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        return services;
    }
    */

    public static IServiceCollection AddBrokerDataContext(this IServiceCollection services, IConfiguration config)
    {
        // var msSqlConnectionString = config.GetConnectionString("MsSqlBrokerDatabase") ?? throw new InvalidOperationException("Connection string 'MsSqlBrokerDatabase' not found.");
        // var pgSqlConnectionString = config.GetConnectionString("PgSqlBrokerDatabase") ?? throw new InvalidOperationException("Connection string 'PgSqlBrokerDatabase' not found.");

        // services.AddDbContext<BrokerDbContext>(options => {
        //     if (msSqlConnectionString is not null && msSqlConnectionString != "")
        //     {
        //         options.UseSqlServer(
        //             config.GetConnectionString("MsSqlBrokerDatabase")!,
        //             x => x.MigrationsAssembly("EdNexusData.Broker.Data.Migrations.SqlServer")
        //         );
        //     }
        //     if (pgSqlConnectionString is not null && pgSqlConnectionString != "")
        //     {
        //         options.UseNpgsql(
        //             config.GetConnectionString("PgSqlBrokerDatabase")!,
        //             x => x.MigrationsAssembly("EdNexusData.Broker.Data.Migrations.PostgreSQL")
        //         );
        //     }
        // }
        // );

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IMediator), typeof(Mediator));

        services.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>(options =>
        {
            options.User.RequireUniqueEmail = false;
        })
        .AddEntityFrameworkStores<BrokerDbContext>()
        .AddTokenProvider<DataProtectorTokenProvider<IdentityUser<Guid>>>(TokenOptions.DefaultProvider);

        
        return services;
    }
    
}