#nullable disable
namespace EdNexusData.Broker.Web.Models.JsonDocuments;

public class RequestManifestJsonModel
{
    public Guid RequestId { get; set; }
    public string RequestType { get; set; }
    public StudentJsonModel Student { get; set; }
    public SchoolJsonModel From { get; set; }
    public SchoolJsonModel To { get; set; }
    public string Note { get; set; }
    public List<string> Contents { get; set; }
}
