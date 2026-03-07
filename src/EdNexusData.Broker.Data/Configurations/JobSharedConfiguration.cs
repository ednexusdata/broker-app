// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core.Worker;

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

        // Force mapping for Job only
        // Explicitly map it to a column
        builder.Property(j => j.CreatedBy).HasColumnName("CreatedBy"); 

        // // If it is a Foreign Key, define the relationship
        builder.HasOne(j => j.CreatedByUser)
            .WithMany()
            .HasForeignKey(j => j.CreatedBy)
            .IsRequired(false);
    }
}
