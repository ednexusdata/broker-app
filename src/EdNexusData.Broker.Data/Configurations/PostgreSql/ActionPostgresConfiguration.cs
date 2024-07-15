// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations.PostgreSql;

internal class ActionPostgresConfiguration : IEntityTypeConfiguration<Domain.Action>
{
    public void Configure(EntityTypeBuilder<Domain.Action> builder)
    {   
        // Json Fields
        builder.Property(i => i.Settings).HasColumnType("jsonb");
    }
}
