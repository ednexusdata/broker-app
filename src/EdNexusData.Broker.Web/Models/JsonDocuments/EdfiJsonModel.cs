#nullable disable
using Newtonsoft.Json;

namespace EdNexusData.Broker.Web.Models.JsonDocuments;

public class EdfiJsonModel
{
    [JsonProperty("EdNexusData.Broker.Connector.EdFiAlliance.EdFi.Student")]
    public EdfiStudentJsonModel Student { get; set; }
}

