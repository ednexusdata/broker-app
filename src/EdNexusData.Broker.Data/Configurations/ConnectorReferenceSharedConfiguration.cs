// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Core.Models;

namespace EdNexusData.Broker.Data.Configurations;

internal class ConnectorReferenceSharedConfiguration : IEntityTypeConfiguration<ConnectorReference>
{
    public void Configure(EntityTypeBuilder<ConnectorReference> builder)
    {   
        // Rename ID to UserId
        builder.Property(i => i.Id).HasColumnName("ConnectorReferenceId");
    }
}
