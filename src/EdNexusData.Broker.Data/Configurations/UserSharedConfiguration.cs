// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations;

internal class UserSharedConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        // Rename ID to UserId
        builder.Property(i => i.Id).HasColumnName("UserId");

        builder.Property(i => i.CreatedAt).ValueGeneratedOnUpdate().Metadata.SetIsStored(true);
        builder.Property(i => i.CreatedBy).ValueGeneratedOnUpdate().Metadata.SetIsStored(true);

        // Set foreign key on primary key since the join is one-to-one to AspNetUsers
        builder.HasOne<IdentityUser<Guid>>().WithOne()
            .HasPrincipalKey<IdentityUser<Guid>>(x => x.Id)
            .HasForeignKey<User>(x => x.Id);
    }
}
