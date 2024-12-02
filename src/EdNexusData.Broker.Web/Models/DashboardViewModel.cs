using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Web.ViewModels;
using EdNexusData.Broker.Web.ViewModels.EducationOrganizations;
using EdNexusData.Broker.Web.ViewModels.IncomingRequests;
using EdNexusData.Broker.Web.ViewModels.OutgoingRequests;

namespace EdNexusData.Broker.Web.Models;

public class DashboardViewModel
{
    public int ReadyIncomingRequests { get; set; }
    public int SentIncomingRequests { get; set; }
    public int ReceivedOutgoingRequestsCount { get; set; }
    public int InProgressOutgoingRequestsCount { get; set; }

    [Display(Name = "Start date")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "Latest Incoming Requests")]
    public IEnumerable<RequestCardViewModel> LatestIncomingRequests { get; set; } = Enumerable.Empty<RequestCardViewModel>();

    [Display(Name = "Latest Outgoing Requests")]
    public IEnumerable<RequestCardViewModel> LatestOutgoingRequests { get; set; } = Enumerable.Empty<RequestCardViewModel>();
}
