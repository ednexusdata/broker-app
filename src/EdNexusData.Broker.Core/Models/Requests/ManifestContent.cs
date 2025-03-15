// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com
namespace EdNexusData.Broker.Core;

public class ManifestContent
{
    public Guid? Id { get; set; }
    public string ContentType { get; set; } = default!;
    public string FileName { get; set; } = default!;

    public Common.Requests.ManifestContent ToCommon()
    {
        return new Common.Requests.ManifestContent()
        {
            Id = Id,
            ContentType = ContentType,
            FileName = FileName
        };
    }
}