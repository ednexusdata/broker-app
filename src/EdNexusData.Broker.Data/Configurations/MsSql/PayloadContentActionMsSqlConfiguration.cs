// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class ActionMsSqlConfiguration : IEntityTypeConfiguration<PayloadContentAction>
{
    public void Configure(EntityTypeBuilder<PayloadContentAction> builder)
    {   
        builder.Property(i => i.Settings).HasColumnType("nvarchar(max)");
    }
}
