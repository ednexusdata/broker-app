// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations;

internal static class SharedEntityTypeConfiguration<T> where T : BaseEntity
{
    public static void ConfigureCreatedUser(EntityTypeBuilder<T> builder)
    {   
        builder.HasOne(x => x.CreatedByUser).WithMany().HasPrincipalKey(x => x.CreatedBy);
    }

    public static void ConfigureUpdatedUser(EntityTypeBuilder<T> builder)
    {   
        builder.HasOne(x => x.UpdatedByUser).WithMany().HasPrincipalKey(x => x.UpdatedBy);
    }

    public static void ConfigureUserStamps(EntityTypeBuilder<T> builder)
    {   
        ConfigureCreatedUser(builder);
        ConfigureUpdatedUser(builder);
    }
}
