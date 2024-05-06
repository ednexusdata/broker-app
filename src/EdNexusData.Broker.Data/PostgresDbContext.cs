using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EdNexusData.Broker.Data;

public class PostgresDbContext : BrokerDbContext
{
    public PostgresDbContext(IConfiguration configuration)
        : base(configuration)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(Configuration.GetConnectionString("PostgreSqlConnection"));
        options.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        var namespaces = new[] { "EdNexusData.Broker.Data.Configurations", "EdNexusData.Broker.Data.Configurations.PostgreSql" };
        ApplyConfiguration(modelBuilder, namespaces);
    }
}