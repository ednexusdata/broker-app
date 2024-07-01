// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain.Worker;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class JobMsSqlConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {   
        // Json Fields
        builder.Property(i => i.JobParameters).HasColumnType("nvarchar(max)");
    }
}
