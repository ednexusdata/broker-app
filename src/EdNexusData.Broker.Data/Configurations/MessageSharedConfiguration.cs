using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core;

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
        builder.Property(i => i.TransmissionDetails).HasJsonConversion();
    }
}
