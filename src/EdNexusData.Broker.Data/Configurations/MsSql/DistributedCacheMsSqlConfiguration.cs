// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class DistributedCacheMsSqlConfiguration : IEntityTypeConfiguration<DistributedCacheEntry>
{
    public void Configure(EntityTypeBuilder<DistributedCacheEntry> builder)
    {
        builder.ToTable("DistributedCache");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
               .HasColumnType("nvarchar(449)")
               .IsRequired();

        builder.Property(e => e.Value)
              .HasColumnType("varbinary(max)")
              .IsRequired();

        builder.Property(e => e.ExpiresAtTime)
              .HasColumnType("datetimeoffset")
              .IsRequired();

        builder.Property(e => e.SlidingExpirationInSeconds)
              .HasColumnType("bigint");

        builder.Property(e => e.AbsoluteExpiration)
              .HasColumnType("datetimeoffset");
    }
}
