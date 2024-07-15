// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using EdNexusData.Broker.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EdNexusData.Broker.Data.Configurations;

internal class ActionSharedConfiguration : IEntityTypeConfiguration<Domain.Action>
{
    public void Configure(EntityTypeBuilder<Domain.Action> builder)
    {   
        builder.ToTable("Actions");
        // Rename ID
        builder.Property(i => i.Id).HasColumnName("ActionId");

        // Json Fields
        builder.Property(i => i.Settings).HasJsonConversion();

        builder.HasOne(d => d.ActiveMapping).WithOne(x => x.Action).HasForeignKey<Domain.Action>(x => x.ActiveMappingId);

        builder.HasIndex(x => new { x.PayloadContentId, x.PayloadContentActionType } ).IsUnique();
    }
}
