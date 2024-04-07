// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

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
    }
}
