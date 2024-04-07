#nullable disable
using Newtonsoft.Json;
namespace EdNexusData.Broker.Web.Models.JsonDocuments;

public class SynergyJsonModel
{
    [JsonProperty("EdNexusData.Broker.Connector.Edupoint.Synergy.Student")]
    public SynergyStudentJsonModel Student { get; set; }
}

