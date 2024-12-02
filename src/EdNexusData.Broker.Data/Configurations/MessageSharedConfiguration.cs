// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations;

internal class MessageSharedConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {  
        builder.ToTable("Messages");
        // Rename ID
        builder.Property(i => i.Id).HasColumnName("MessageId");

        // Json Fields
        builder.Property(i => i.MessageContents).HasJsonConversion();
        builder.Property(i => i.Sender).HasJsonConversion();
        builder.Property(i => i.TransmissionDetails).HasJsonConversion();
    }
}
