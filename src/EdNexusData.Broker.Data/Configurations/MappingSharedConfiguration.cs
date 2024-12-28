// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations;

internal class MappingSharedConfiguration : IEntityTypeConfiguration<Mapping>
{
    public void Configure(EntityTypeBuilder<Mapping> builder)
    {   
        builder.ToTable("Mappings");
        // Rename ID
        builder.Property(i => i.Id).HasColumnName("MappingId");

        // Json Fields
        builder.Property(i => i.OriginalSchema).HasJsonConversion();
        builder.Property(i => i.StudentAttributes).HasJsonConversion();
        builder.Property(i => i.JsonInitialMapping).HasJsonConversion();
        builder.Property(i => i.JsonInitialMapping).HasJsonConversion();
        builder.Property(i => i.JsonSourceMapping).HasJsonConversion();
        builder.Property(i => i.JsonDestinationMapping).HasJsonConversion();

        builder.Property(i => i.Version).HasDefaultValue(1);

        builder.HasIndex(x => new { x.PayloadContentActionId, x.Version } ).IsUnique();
    }
}
