using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Web.ViewModels.Requests;

public class RequestViewModel
{
    public Request? Request { get; set; }
    public List<PayloadContent>? RequestingPayloadContents { get; set; }
    public List<PayloadContent>? ReleasingPayloadContents { get; set; }
    public Dictionary<RequestStatus, StatusGridViewModel> StatusGrid { get; set; } = new();
}
