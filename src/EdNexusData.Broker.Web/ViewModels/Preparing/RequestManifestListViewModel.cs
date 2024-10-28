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
    public TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
    public DateTimeOffset ReceivedDate { get; set; } = default!;
    public string? ReceivedDateDisplay { get {
        return TimeZoneInfo.ConvertTimeFromUtc(this.ReceivedDate.DateTime, timeZoneInfo).ToString("M/dd/yyyy h:mm tt");
    } }
    public string FileName { get; set; } = default!;
    public string ContentCategory { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public int? ReceviedCount { get; set; }
    public int? IgnoredCount { get; set; }
    public int? MappedCount { get; set; }
    public int? RemainingCount { get { return ReceviedCount - IgnoredCount - MappedCount; } }
    public Guid PayloadContentId { get; set; }
    public PayloadContentAction? Action { get; set; }
    public string? PayloadContentActionType { get; set; }
}