// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations;

internal class EducationOrganizationPayloadSettingsSharedConfiguration : IEntityTypeConfiguration<EducationOrganizationPayloadSettings>
{
    public void Configure(EntityTypeBuilder<EducationOrganizationPayloadSettings> builder)
    {   
        // Rename ID to UserId
        builder.Property(i => i.Id).HasColumnName("EducationOrganizationPayloadSettingsId");

        // Settings is json
        // builder.OwnsOne(i => i.IncomingPayloadSettings, 
        //                 nv => { 
        //                    nv.ToJson();
        //                    nv.OwnsMany(x => x.PayloadContents, z => {
        //                      z.ToJson();
        //                      z.OwnsOne(y => y.Settings);
        //                    });
        //                 }
        //             );
        // builder.OwnsOne(i => i.OutgoingPayloadSettings, 
        //                 nv => { 
        //                    nv.ToJson();
        //                    nv.OwnsMany(x => x.PayloadContents, z => {
        //                      z.ToJson();
        //                      z.OwnsOne(y => y.Settings);
        //                    });
        //                 }
        //             );
        builder.OwnsOne(i => i.IncomingPayloadSettings, nv => { nv.ToJson(); nv.OwnsMany(i => i.PayloadContents); });
        builder.OwnsOne(i => i.OutgoingPayloadSettings, nv => { nv.ToJson(); nv.OwnsMany(i => i.PayloadContents); });

        //builder.Property(i => i.IncomingPayloadSettings).HasJsonConversion();
        //builder.Property(i => i.OutgoingPayloadSettings).HasJsonConversion();

        // Create unique key constraint for EducationOrganizationid and UserId
        builder.HasIndex(x => new { x.EducationOrganizationId, x.Payload } ).IsUnique();
    }
}
