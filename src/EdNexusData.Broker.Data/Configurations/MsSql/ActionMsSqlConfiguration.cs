// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class ActionMsSqlConfiguration : IEntityTypeConfiguration<Domain.Action>
{
    public void Configure(EntityTypeBuilder<Domain.Action> builder)
    {   
        builder.Property(i => i.Settings).HasColumnType("nvarchar(max)");
    }
}
