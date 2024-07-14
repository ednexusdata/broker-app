using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Connector.PayloadContentActions;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Extensions;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EdNexusData.Broker.Web.ViewModels.Preparing;

public class RequestManifestListViewModel
{
    public Guid RequestId { get; set; }
    public RequestStatus RequestStatus { get; set; }
    
    public List<RequestManifestViewModel> PayloadContents { get; set; } = new List<RequestManifestViewModel>();

    public List<SelectListItem> PayloadContentActions { get; set; } = new List<SelectListItem>();
}

public class RequestManifestViewModel
{
    public DateTimeOffset ReceivedDate { get; set; } = default!;
    public string? ReceivedDateDisplay { get {
        var pacific = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(this.ReceivedDate.DateTime, pacific).ToString("M/dd/yyyy h:mm tt");
    } }
    public string FileName { get; set; } = default!;
    public string ContentCategory { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public int? ReceviedCount { get; set; }
    public int? MappedCount { get; set; }
    public Guid PayloadContentId { get; set;}
    public PayloadContentAction PayloadContentAction { get; set; } = new PayloadContentAction() { ConnectorAction = nameof(IgnorePayloadContentAction) };
}