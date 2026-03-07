// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.ComponentModel.DataAnnotations.Schema;

namespace EdNexusData.Broker.Core;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    [NotMapped]
    public User? CreatedByUser { get; set; }

    [NotMapped]
    public User? UpdatedByUser { get; set; }
}