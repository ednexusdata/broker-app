using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class ActivityLogMsSqlConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.Property(i => i.Metadata).HasColumnType("nvarchar(max)");
    }
}
