// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
namespace EdNexusData.Broker.Domain;

public class PayloadSettingsContentType
{
    public Guid JobId { get; set; }
    public string PayloadContentType { get; set; } = default!;
    public string? Settings { get; set; }
}
