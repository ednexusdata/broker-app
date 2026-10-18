// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations;

internal class EducationOrganizationDomainSharedConfiguration : IEntityTypeConfiguration<EducationOrganizationDomain>
{
    public void Configure(EntityTypeBuilder<EducationOrganizationDomain> builder)
    {
        builder.ToTable("EducationOrganizationDomains");
        
        builder.Property(i => i.Id).HasColumnName("EducationOrganizationDomainId");

        // Domains must be unique across every district in the broker
        builder.HasIndex(x => x.Domain).IsUnique();

        builder.HasOne(d => d.EducationOrganization)
            .WithMany(e => e.Domains)
            .HasForeignKey(d => d.EducationOrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
