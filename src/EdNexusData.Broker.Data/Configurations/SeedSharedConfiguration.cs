// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations;

internal class SeedSharedConfiguration : IEntityTypeConfiguration<Seed>
{
    public void Configure(EntityTypeBuilder<Seed> builder)
    { 
        builder.ToTable("__BrokerSeedsHistory");
    }
}
