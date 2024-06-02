// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations.PostgreSql;

internal class PayloadContentPostgresConfiguration : IEntityTypeConfiguration<PayloadContent>
{
    public void Configure(EntityTypeBuilder<PayloadContent> builder)
    {   
        builder.Property(i => i.JsonContent).HasColumnType("jsonb");
        builder.Property(i => i.Actions).HasColumnType("jsonb");
    }
}
