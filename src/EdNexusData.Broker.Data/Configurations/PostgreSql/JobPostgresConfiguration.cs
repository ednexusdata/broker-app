// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Data.Configurations.PostgreSql;

internal class JobPostgresConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {   
        // Json Fields
        builder.Property(i => i.JobParameters).HasColumnType("jsonb");
    }
}
