// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations;

internal class EducationOrganizationPayloadSettingsSharedConfiguration : IEntityTypeConfiguration<EducationOrganizationPayloadSettings>
{
    public void Configure(EntityTypeBuilder<EducationOrganizationPayloadSettings> builder)
    {   
        // Rename ID to UserId
        builder.Property(i => i.Id).HasColumnName("EducationOrganizationPayloadSettingsId");

        // Settings is json
        builder.OwnsOne(i => i.IncomingPayloadSettings, nv => { nv.ToJson(); nv.OwnsMany(i => i.PayloadContents); });
        builder.OwnsOne(i => i.OutgoingPayloadSettings, nv => { nv.ToJson(); nv.OwnsMany(i => i.PayloadContents); });

        // Create unique key constraint for EducationOrganizationid and UserId
        builder.HasIndex(x => new { x.EducationOrganizationId, x.Payload } ).IsUnique();
    }
}
