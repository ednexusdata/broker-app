using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations;

internal class ActivityLogSharedConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLog");
        // Rename ID
        builder.Property(i => i.Id).HasColumnName("ActivityLogId");

        // Json Fields
        builder.Property(i => i.Metadata).HasJsonConversion();

        builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        builder.HasIndex(x => x.RequestId);
        builder.HasIndex(x => x.ActivityType);

        // UserId is nullable for system/worker-initiated activity, so this FK is naturally optional.
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // RequestCleanupJob hard-deletes Request rows once retention expires. Its activity log entries
        // (including the final "deleted" entry, logged before the delete) get baked into the Proof of
        // Request PDF as the last section first, so cascading them away here doesn't lose that history —
        // it just stops them from lingering as orphaned, unlinkable rows once the request is gone.
        builder.HasOne(x => x.Request)
            .WithMany()
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
