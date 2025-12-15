// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations;

internal class EducationOrganizationSharedConfiguration : IEntityTypeConfiguration<EducationOrganization>
{
    public void Configure(EntityTypeBuilder<EducationOrganization> builder)
    {   
        // Rename ID to UserId
        builder.Property(i => i.Id).HasColumnName("EducationOrganizationId");

        builder.Property(i => i.Address).HasJsonConversion();
        builder.Property(i => i.Contacts).HasJsonConversion();

        builder.HasIndex(x => new { x.Domain } ).IsUnique();

        builder.HasOne(e => e.ParentOrganization) 
            .WithMany()                              
            .HasForeignKey("ParentOrganizationId");

        builder.HasOne(e => e.ParentOrganization)
            .WithMany(e => e.EducationOrganizations)
            .HasForeignKey(e => e.ParentOrganizationId);
    }
}
