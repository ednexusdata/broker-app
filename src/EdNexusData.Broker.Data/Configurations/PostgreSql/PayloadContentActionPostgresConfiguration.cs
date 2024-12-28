// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations.PostgreSql;

internal class ActionPostgresConfiguration : IEntityTypeConfiguration<PayloadContentAction>
{
    public void Configure(EntityTypeBuilder<PayloadContentAction> builder)
    {   
        // Json Fields
        builder.Property(i => i.Settings).HasColumnType("jsonb");
    }
}
