// using Microsoft.EntityFrameworkCore.Design;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;

// namespace EdNexusData.Broker.Data;

// public class ContractContextFactory : IDesignTimeDbContextFactory<BrokerDbContext>
// {
//     public BrokerDbContext CreateDbContext(string[] args)
//     {
//         IConfigurationRoot config = new ConfigurationBuilder()
//             .AddJsonFile("appsettings.json", false)
//             .Build();
        
//         var msSqlConnectionString = config.GetConnectionString("MsSqlBrokerDatabase") ?? throw new InvalidOperationException("Connection string 'MsSqlBrokerDatabase' not found.");
//         var pgSqlConnectionString = config.GetConnectionString("PgSqlBrokerDatabase") ?? throw new InvalidOperationException("Connection string 'PgSqlBrokerDatabase' not found.");
        
//         var optionsBuilder = new DbContextOptionsBuilder<BrokerDbContext>();

//         if (msSqlConnectionString is not null && msSqlConnectionString != "")
//         {
//             optionsBuilder.UseSqlServer(
//                 config.GetConnectionString("MsSqlBrokerDatabase")!,
//                 x => x.MigrationsAssembly("EdNexusData.Broker.Data.Migrations.SqlServer")
//             );
//         }
//         if (pgSqlConnectionString is not null && pgSqlConnectionString != "")
//         {
//             optionsBuilder.UseNpgsql(
//                 config.GetConnectionString("PgSqlBrokerDatabase")!,
//                 x => x.MigrationsAssembly("EdNexusData.Broker.Data.Migrations.PostgreSQL")
//             );
//         }

//        return new BrokerDbContext(optionsBuilder.Options);
//     }
// }
