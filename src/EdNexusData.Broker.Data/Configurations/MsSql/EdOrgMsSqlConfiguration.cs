// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EdNexusData.Broker.Domain;

namespace EdNexusData.Broker.Data.Configurations.MsSql;

internal class EdOrgMsSqlConfiguration : IEntityTypeConfiguration<EducationOrganization>
{
    public void Configure(EntityTypeBuilder<EducationOrganization> builder)
    {   
        // Json Fields
        builder.Property(i => i.Address).HasColumnType("nvarchar(max)");
        builder.Property(i => i.Contacts).HasColumnType("nvarchar(max)");
    }
}
