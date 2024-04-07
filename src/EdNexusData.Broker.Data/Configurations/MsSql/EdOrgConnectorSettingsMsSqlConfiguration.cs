// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class EducationOrganizationPayloadSettingsMsSqlConfiguration : IEntityTypeConfiguration<EducationOrganizationConnectorSettings>
{
    public void Configure(EntityTypeBuilder<EducationOrganizationConnectorSettings> builder)
    {   
        // Settings is nvarchar(max)
        builder.Property(i => i.Settings).HasColumnType("nvarchar(max)");
    }
}
