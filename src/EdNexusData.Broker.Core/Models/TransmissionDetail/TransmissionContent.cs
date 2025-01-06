using System.Net;
using Microsoft.Extensions.Primitives;

namespace EdNexusData.Broker.Core;

public class TransmissionContent
{
    public Dictionary<string, StringValues> Headers { get; set; } = new Dictionary<string, StringValues>();
    public Dictionary<string, IEnumerable<string>> StringHeaders { get; set; } = new Dictionary<string, IEnumerable<string>>();
    public int? StatusCode { get; set; }
    public HttpStatusCode? HttpStatusCode { get; set; }
    public string? Content { get; set; }
}