// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations;

internal class UserRoleSharedConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    { 
        builder.ToTable("UserRoles");
        
        // Rename ID to UserId
        builder.Property(i => i.Id).HasColumnName("UserRoleId");

        // Create unique key constraint for EducationOrganizationid and UserId
        builder.HasIndex(x => new { x.EducationOrganizationId, x.UserId } ).IsUnique();

        builder.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
        builder.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    }
}
