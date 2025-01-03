using System.Net;

namespace EdNexusData.Broker.Core;

public class TransmissionContent
{
    public Dictionary<string, IEnumerable<string>> Headers { get; set; } = new Dictionary<string, IEnumerable<string>>();
    public HttpStatusCode? StatusCode { get; set; }
    public string? Content { get; set; }
}