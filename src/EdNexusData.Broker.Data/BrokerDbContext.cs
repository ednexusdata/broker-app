// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EdNexusData.Broker.Core;
using Microsoft.Extensions.Configuration;
using EdNexusData.Broker.Core.Worker;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace EdNexusData.Broker.Data;

public class BrokerDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>, IDataProtectionKeyContext
{
    //private readonly IMediator _mediator;
    protected readonly IConfiguration Configuration;
    
    public BrokerDbContext(IConfiguration configuration) // , IMediator mediator
    {
        Configuration = configuration;
        //_mediator = mediator;
    }

    public DbSet<EducationOrganization>? EducationOrganizations { get; set; }
    public DbSet<User>? ApplicationUsers { get; set; }
    public DbSet<Request>? Requests { get; set; }
    public DbSet<Job>? WorkerJobs { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    public DbSet<Seed>? Seeds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    protected void ApplyConfiguration(ModelBuilder modelBuilder, string[] namespaces)
    {
        var methodInfo = typeof(ModelBuilder).GetMethods().Single((e =>
            e.Name == "ApplyConfiguration" &&
            e.ContainsGenericParameters &&
            e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition() ==
            typeof(IEntityTypeConfiguration<>)));

        foreach (var configType in typeof(BrokerDbContext)
                     .GetTypeInfo().Assembly
                     .GetTypes()
                     .Where(t => t.Namespace != null &&
                                 namespaces.Any(n => n == t.Namespace) &&
                                 t.GetInterfaces().Any(i => i.IsGenericType &&
                                                            i.GetGenericTypeDefinition() ==
                                                            typeof(IEntityTypeConfiguration<>)
                                 )
                     )
                )
        {
            var type = configType.GetInterfaces().First();
            methodInfo.MakeGenericMethod(type.GenericTypeArguments[0]).Invoke(modelBuilder, new[]
            {
                Activator.CreateInstance(configType)
            });
        }
    }
}
