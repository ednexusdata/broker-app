// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations.PostgreSql;

internal class EducationOrganizationPayloadSettingsPostgresConfiguration : IEntityTypeConfiguration<EducationOrganizationConnectorSettings>
{
    public void Configure(EntityTypeBuilder<EducationOrganizationConnectorSettings> builder)
    {   
        // Settings is json
        builder.Property(i => i.Settings).HasColumnType("jsonb");
    }
}
