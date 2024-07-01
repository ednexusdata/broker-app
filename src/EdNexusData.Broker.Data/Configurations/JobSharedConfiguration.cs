// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain.Worker;

namespace EdNexusData.Broker.Data.Configurations;

internal class JobSharedConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {   
        builder.ToTable("Worker_Jobs");
        // Rename ID
        builder.Property(i => i.Id).HasColumnName("JobId");

        // Json Fields
        builder.Property(i => i.JobParameters).HasJsonConversion();
    }
}
