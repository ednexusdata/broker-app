// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations;

internal class DistributedCacheSharedConfiguration : IEntityTypeConfiguration<DistributedCacheEntry>
{
    public void Configure(EntityTypeBuilder<DistributedCacheEntry> builder)
    {   
        builder.ToTable("DistributedCache");
        builder.HasKey(e => e.Id);
    }
}
