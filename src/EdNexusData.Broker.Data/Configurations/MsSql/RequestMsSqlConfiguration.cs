// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class RequestMsSqlConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {   
        // Json Fields
        builder.Property(i => i.Student).HasColumnType("nvarchar(max)");
        builder.Property(i => i.RequestManifest).HasColumnType("nvarchar(max)");
        builder.Property(i => i.ResponseManifest).HasColumnType("nvarchar(max)");
    }
}
