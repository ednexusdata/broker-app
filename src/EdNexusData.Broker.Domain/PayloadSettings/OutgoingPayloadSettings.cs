// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

namespace EdNexusData.Broker.Domain;

public class OutgoingPayloadSettings
{
    public string? StudentLookupConnector { get; set; }
    public List<PayloadSettingsContentType>? PayloadContents { get; set; }

    public Core.PayloadSettings.OutgoingPayloadSettings ToContract()
    {
        var payloadContents = new List<Core.PayloadContentActions.PayloadSettingsContentType>();

        if (PayloadContents is not null)
        {
            foreach(var payloadContent in PayloadContents)
            {
                payloadContents.Add(new Core.PayloadContentActions.PayloadSettingsContentType() {
                    JobId = payloadContent.JobId,
                    PayloadContentType = payloadContent.PayloadContentType,
                    Settings = payloadContent.Settings
                });
            }
        }
        
        return new Core.PayloadSettings.OutgoingPayloadSettings()
        {
            StudentLookupConnector = StudentLookupConnector,
            PayloadContents = payloadContents
        };
    }
}
