// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Models;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.ViewModels;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize]
public class HomeController : AuthenticatedController<HomeController>
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<Request> _requestRepository;
    private readonly FocusHelper _focusHelper;
    private readonly CurrentUserHelper currentUserHelper;

    public HomeController(
        IHttpContextAccessor httpContextAccessor,
        IRepository<User> userRepository,
        IRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<Request> requestRepository,
        ILogger<HomeController> logger,
        FocusHelper focusHelper,
        CurrentUserHelper currentUserHelper)
    {
        _userRepository = userRepository;
        _educationOrganizationRepository = educationOrganizationRepository;
        _requestRepository = requestRepository;
        _logger = logger;
        _focusHelper = focusHelper;
        this.currentUserHelper = currentUserHelper;
    }

    public async Task<IActionResult> Index(
        DashboardViewModel model,
        CancellationToken cancellationToken)
    {
        // TODO: Refactor and optimize dashboard queries, using dirty temporary ops for mockup purposes.

        // IMPORTANT: Everything should be filtered by current Focus
        // Some queries can be omitted based on user role.

        // We will need to support a date range (start and end dates) to filter out the data.
        // Currently displaying last 7 days, last 30 days, and All-time.
        // To support All-time, the end date should be nullable.
        // For example, Outgoing Processor does not need to see incoming record request data.

        var requests = await _requestRepository.ListAsync(new RequestsStartedForSchoolsSpec(await _focusHelper.GetFocusedSchools(), model.StartDate));

        // Only take 5, displaying latest incoming requests
        // Need the total count as well
        var readyIncomingRequests = requests
            .Where(request => request.IncomingOutgoing == IncomingOutgoing.Incoming
             && request.RequestStatus == RequestStatus.Received);

        var sentIncomingRequests = requests
            .Where(request => request.IncomingOutgoing == IncomingOutgoing.Incoming
            && request.RequestStatus == RequestStatus.Requested);

        // Only take 5, displaying latest outgoing requests
        // Need the total count as well
        var inProgressOutgoingRequests = requests
            .Where(request => request.IncomingOutgoing == IncomingOutgoing.Outgoing
            && request.RequestStatus == RequestStatus.Extracted);

        // Only take 5, displaying latest outgoing requests
        // Need the total count as well
        var receivedOutgoingRequests = requests
            .Where(request => request.IncomingOutgoing == IncomingOutgoing.Outgoing
            && request.RequestStatus == RequestStatus.Received);

        // Temporary, taking 10 here
        var incomingRequestViewModels = readyIncomingRequests
            .Take(10)
            .Select(incomingRequest =>  new RequestCardViewModel(incomingRequest, currentUserHelper.ResolvedCurrentUserTimeZone()))
            .ToList();

        // Temporary, taking 10 here
        var outgoingRequestViewModels = receivedOutgoingRequests
            .Take(10)
            .Select(outgoingRequest => new RequestCardViewModel(outgoingRequest, currentUserHelper.ResolvedCurrentUserTimeZone()))
            .ToList();

        model.ReadyIncomingRequests = readyIncomingRequests.Count();
        model.SentIncomingRequests = sentIncomingRequests.Count();
        model.ReceivedOutgoingRequestsCount = receivedOutgoingRequests.Count();
        model.InProgressOutgoingRequestsCount = inProgressOutgoingRequests.Count();
        model.LatestIncomingRequests = incomingRequestViewModels;
        model.LatestOutgoingRequests = outgoingRequestViewModels;
        model.StartDate = model.StartDate;  

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
