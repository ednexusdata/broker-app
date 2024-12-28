// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

namespace EdNexusData.Broker.Domain;

public class OutgoingPayloadSettings
{
    public string? StudentLookupConnector { get; set; }
    public List<PayloadSettingsContentType>? PayloadContents { get; set; }

    public Common.PayloadSettings.OutgoingPayloadSettings ToCommon()
    {
        var payloadContents = new List<Common.PayloadContentActions.PayloadSettingsContentType>();

        if (PayloadContents is not null)
        {
            foreach(var payloadContent in PayloadContents)
            {
                payloadContents.Add(new Common.PayloadContentActions.PayloadSettingsContentType() {
                    JobId = payloadContent.JobId,
                    PayloadContentType = payloadContent.PayloadContentType,
                    Settings = payloadContent.Settings
                });
            }
        }
        
        return new Common.PayloadSettings.OutgoingPayloadSettings()
        {
            StudentLookupConnector = StudentLookupConnector,
            PayloadContents = payloadContents
        };
    }
}
