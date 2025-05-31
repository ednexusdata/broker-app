// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations;

internal class SettingsSharedConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {   
        // Rename ID to UserId
        builder.Property(i => i.Id).HasColumnName("SettingId");

        builder.HasIndex(x => new { x.Key } ).IsUnique();
    }
}
