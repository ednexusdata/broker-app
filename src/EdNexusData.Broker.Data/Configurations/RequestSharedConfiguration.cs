// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations;

internal class RequestSharedConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {   
        // Rename ID
        builder.Property(i => i.Id).HasColumnName("RequestId");

        // Json Fields
        builder.Property(i => i.Student).HasJsonConversion();
        builder.Property(i => i.RequestManifest).HasJsonConversion();
        builder.Property(i => i.ResponseManifest).HasJsonConversion();
    }
}
