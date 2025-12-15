using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EdNexusData.Broker.Data;

public class MsSqlDbContext : BrokerDbContext
{
    public MsSqlDbContext(IConfiguration configuration) : base(configuration)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {   
        options.UseSqlServer(Configuration.GetConnectionString("MsSqlConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        //modelBuilder.Entity<CacheEntry>().ToTable("DistributedCache");

        var namespaces = new[] { "EdNexusData.Broker.Data.Configurations", "EdNexusData.Broker.Data.Configurations.MsSql" };
        ApplyConfiguration(modelBuilder, namespaces);
    }
}
