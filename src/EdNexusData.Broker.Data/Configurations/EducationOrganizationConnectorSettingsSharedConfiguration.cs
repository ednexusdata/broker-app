// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations;

internal class EducationOrganizationConnectorSettingsSharedConfiguration : IEntityTypeConfiguration<EducationOrganizationConnectorSettings>
{
    public void Configure(EntityTypeBuilder<EducationOrganizationConnectorSettings> builder)
    {   
        // Rename ID to UserId
        builder.Property(i => i.Id).HasColumnName("EducationOrganizationConnectorSettingsId");

        builder.Property(i => i.Settings).HasJsonConversion();

        // Create unique key constraint for EducationOrganizationid and UserId
        builder.HasIndex(x => new { x.EducationOrganizationId, x.Connector } ).IsUnique();
    }
}
