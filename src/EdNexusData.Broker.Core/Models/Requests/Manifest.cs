// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
namespace EdNexusData.Broker.Core;

public class Manifest
{
    public Guid? RequestId { get; set; }
    public string RequestType { get; set; } = default!;
    public string MessageType { get; set; } = typeof(Manifest).FullName!;
    public Student? Student { get; set; }
    public RequestAddress? From { get; set; }
    public RequestAddress? To { get; set; }
    public string? Note { get; set; }
    public List<ManifestContent>? Contents { get; set; } = new List<ManifestContent>();

    public Common.Requests.Manifest ToCommon()
    {
        return new Common.Requests.Manifest()
        {
            RequestType = RequestType,
            MessageType = MessageType,
            Note = Note,
            From = From?.ToCommon(),
            To = To?.ToCommon(),
            Contents = Contents?.Select(x => x.ToCommon()).ToList()
        };
    }
}