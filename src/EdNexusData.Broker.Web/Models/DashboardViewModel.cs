using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Web.ViewModels.EducationOrganizations;
using EdNexusData.Broker.Web.ViewModels.IncomingRequests;
using EdNexusData.Broker.Web.ViewModels.OutgoingRequests;

namespace EdNexusData.Broker.Web.Models;

public class DashboardViewModel
{
    [Display(Name = "Initial Requests Count")]
    public int InitialRequestsCount { get; set; }

    [Display(Name = "Outgoing Requests Count")]
    public int OutgoingRequestsCount { get; set; }

    [Display(Name = "Draft Requests Count")]
    public int DraftCount { get; set; }

    [Display(Name = "Waiting Approval Requests Count")]
    public int WaitingApprovalCount { get; set; }

    [Display(Name = "Approved Requests Count")]
    public int ApprovedCount { get; set; }

    [Display(Name = "Declined Requests Count")]
    public int DeclinedCount { get; set; }

    [Display(Name = "Start date")]
    public DateTime? StartDate { get; set; }

    [Display(Name = "Latest Incoming Requests")]
    public IEnumerable<IncomingRequestViewModel> LatestIncomingRequests { get; set; } = Enumerable.Empty<IncomingRequestViewModel>();

    [Display(Name = "Latest Outgoing Requests")]
    public IEnumerable<OutgoingRequestViewModel> LatestOutgoingRequests { get; set; } = Enumerable.Empty<OutgoingRequestViewModel>();

    [Display(Name = "Education Organizations")]
    public IEnumerable<EducationOrganizationRequestViewModel> EducationOrganizations { get; set; } = Enumerable.Empty<EducationOrganizationRequestViewModel>();
}
