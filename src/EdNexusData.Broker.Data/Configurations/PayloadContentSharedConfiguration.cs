// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Xml.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations;

internal class PayloadContentSharedConfiguration : IEntityTypeConfiguration<PayloadContent>
{
    public void Configure(EntityTypeBuilder<PayloadContent> builder)
    {   
        builder.ToTable("PayloadContents");
        // Rename ID
        builder.Property(i => i.Id).HasColumnName("PayloadContentId");

        // Json Fields
        builder.Property(i => i.JsonContent).HasJsonConversion();
        builder.Property(i => i.Actions).HasJsonConversion();

        // Set to xml
        builder.Property(e => e.XmlContent)
            .HasConversion(
                xml => xml!.ToString(),
                xml => xml != null ? XElement.Parse(xml) : null)
            .HasColumnType("xml");
    }
}
