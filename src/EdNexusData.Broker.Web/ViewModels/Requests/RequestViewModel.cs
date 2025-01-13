using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Web.Helpers;

namespace EdNexusData.Broker.Web.ViewModels.Requests;

public class RequestViewModel
{
    public Request? Request { get; set; }
    public List<PayloadContent>? RequestingPayloadContents { get; set; }
    public List<PayloadContent>? ReleasingPayloadContents { get; set; }
    public Dictionary<RequestStatus, StatusGridViewModel> StatusGrid { get; set; } = new();
    public DisplayMessageType DisplayMessagesType { get; set; } = DisplayMessageType.TransmissionMessages;

    public enum DisplayMessageType
    {
        ChatMessages,
        TransmissionMessages
    }

    public void SetStatusGrid(CurrentUserHelper currentUserHelper)
    {
        var statusGrid = new Dictionary<RequestStatus, StatusGridViewModel>();
        
        if (Request?.Messages is null) return;
        
        foreach(var message in Request?.Messages!)
        {
            // var messageType = message.MessageContents?.RootElement.GetProperty("MessageType").GetString();
            // var deseralizedMessageContent = JsonConvert.DeserializeObject(message.MessageContents.ToJsonString()!, Type.GetType(messageType!)!);
            if (message.RequestStatus is not null && !statusGrid.ContainsKey(message.RequestStatus.Value))
            {
                if (message.SentTimestamp is not null)
                {
                    statusGrid[message.RequestStatus.Value] = new StatusGridViewModel()
                    {
                        Timestamp = TimeZoneInfo.ConvertTimeFromUtc(
                            message.SentTimestamp!.Value.DateTime, 
                            currentUserHelper.ResolvedCurrentUserTimeZone()
                        ).ToString("M/dd/yyyy h:mm tt"),
                        Userstamp = message.MessageContents?.Sender?.Name
                    };
                }
            }
        }
        StatusGrid = statusGrid;
    }
}