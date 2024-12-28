// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
using System.Text.Json.Serialization;

namespace EdNexusData.Broker.Core;

public class StudentRequest
{
    [JsonPropertyName("EdNexusData.Broker.Core.Student")]
    public Student? Student { get; set; }

    public Dictionary<string, object>? Connectors { get; set; }
}