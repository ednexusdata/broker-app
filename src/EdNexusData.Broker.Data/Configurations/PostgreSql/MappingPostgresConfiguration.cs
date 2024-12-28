// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations.PostgreSql;

internal class MappingPostgresConfiguration : IEntityTypeConfiguration<Mapping>
{
    public void Configure(EntityTypeBuilder<Mapping> builder)
    {   
        // Json Fields
        builder.Property(i => i.StudentAttributes).HasColumnType("jsonb");
        builder.Property(i => i.OriginalSchema).HasColumnType("jsonb");
        builder.Property(i => i.JsonSourceMapping).HasColumnType("jsonb");
        builder.Property(i => i.JsonInitialMapping).HasColumnType("jsonb");
        builder.Property(i => i.JsonDestinationMapping).HasColumnType("jsonb");
    }
}
