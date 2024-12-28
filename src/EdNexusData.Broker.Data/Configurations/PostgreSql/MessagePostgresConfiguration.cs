// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

namespace EdNexusData.Broker.Data.Configurations.PostgreSql;

internal class MessagePostgresConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {   
        // Json Fields
        builder.Property(i => i.MessageContents).HasColumnType("jsonb");
        builder.Property(i => i.Sender).HasColumnType("jsonb");
        builder.Property(i => i.TransmissionDetails).HasColumnType("jsonb");
    }
}
