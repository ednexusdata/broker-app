// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class MessageMsSqlConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {   
        // Json Fields
        builder.Property(i => i.MessageContents).HasColumnType("nvarchar(max)");
        builder.Property(i => i.Sender).HasColumnType("nvarchar(max)");
        builder.Property(i => i.TransmissionDetails).HasColumnType("nvarchar(max)");
    }
}
