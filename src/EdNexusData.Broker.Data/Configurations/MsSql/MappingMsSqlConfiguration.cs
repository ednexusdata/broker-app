// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class MappingMsSqlConfiguration : IEntityTypeConfiguration<Mapping>
{
    public void Configure(EntityTypeBuilder<Mapping> builder)
    {   
        builder.Property(i => i.StudentAttributes).HasColumnType("nvarchar(max)");
        builder.Property(i => i.OriginalSchema).HasColumnType("nvarchar(max)");
        builder.Property(i => i.JsonSourceMapping).HasColumnType("nvarchar(max)");
        builder.Property(i => i.JsonInitialMapping).HasColumnType("nvarchar(max)");
        builder.Property(i => i.JsonDestinationMapping).HasColumnType("nvarchar(max)");
    }
}
