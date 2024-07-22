// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using EdNexusData.Broker.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EdNexusData.Broker.Data.Configurations;

internal class PayloadContentActionSharedConfiguration : IEntityTypeConfiguration<PayloadContentAction>
{
    public void Configure(EntityTypeBuilder<PayloadContentAction> builder)
    {   
        builder.ToTable("PayloadContentActions");
        // Rename ID
        builder.Property(i => i.Id).HasColumnName("PayloadContentActionId");

        // Json Fields
        builder.Property(i => i.Settings).HasJsonConversion();

        builder.HasMany(i => i.Mappings).WithOne(i => i.PayloadContentAction).HasForeignKey(x => x.PayloadContentActionId);
        
        builder.HasOne(d => d.ActiveMapping).WithOne(x => x.PrimaryPayloadContentAction).HasForeignKey<PayloadContentAction>(x => x.ActiveMappingId);

        builder.HasIndex(x => new { x.PayloadContentId, x.PayloadContentActionType } ).IsUnique();
    }
}
