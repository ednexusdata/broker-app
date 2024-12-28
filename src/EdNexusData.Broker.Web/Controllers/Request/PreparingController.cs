using System.ComponentModel;
using Ardalis.Specification;
using EdNexusData.Broker.Common;
using EdNexusData.Broker.Common.Connector;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Service;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.ViewModels.Preparing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferOutgoingRecords)]
public class PreparingController : AuthenticatedController<RequestsController>
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Domain.PayloadContent> _payloadContentRepository;
    private readonly IRepository<PayloadContentAction> _actionRepository;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly JobService _jobService;
    private readonly CurrentUserHelper currentUserHelper;
    private readonly ConnectorLoader _connectorLoader;

    public PreparingController(
        IRepository<Request> requestRepository, 
        IRepository<Domain.PayloadContent> payloadContentRepository, 
        IRepository<PayloadContentAction> actionRepository,
        IRepository<Mapping> mappingRepository,
        ConnectorLoader connectorLoader,
        JobService jobService,
        CurrentUserHelper currentUserHelper)
    {
        _requestRepository = requestRepository;
        _payloadContentRepository = payloadContentRepository;
        _actionRepository = actionRepository;
        _connectorLoader = connectorLoader;
        _mappingRepository = mappingRepository;
        _actionRepository = actionRepository;
        _jobService = jobService;
        this.currentUserHelper = currentUserHelper;
    }

    [Route("/Preparing/{id:guid}")]
    public async Task<IActionResult> Index(Guid id, Guid? jobId)
    {
        // Get all payload content files for request
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdWithMessagesPayloadContents(id));
        if (request == null)
        {
            return NotFound();
        }

        if (request.RequestStatus.NotIn(RequestStatus.InProgress))
        {
            ViewBag.JobId = jobId;
        }

        var viewModel = new RequestManifestListViewModel
        {
            RequestId = id,
            RequestStatus = request.RequestStatus
        };

        foreach (var contentPayloadAction in _connectorLoader.GetPayloadContentActions()!)
        {
            var connectorType = contentPayloadAction.Assembly.GetExportedTypes().Where(p => p.IsAssignableTo(typeof(IConnector))).FirstOrDefault();

            var displayNameType = connectorType?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault();
            var displayName = "";
            if (displayNameType is not null)
            {
                displayName = ((DisplayNameAttribute)displayNameType).DisplayName + " / ";
            }

            viewModel.PayloadContentActions.Add(
                new SelectListItem() {
                    Text = displayName + contentPayloadAction.GetProperty("DisplayName")?.GetValue(null, null)?.ToString(),
                    Value = contentPayloadAction.FullName
                }
            );
        }

        viewModel.PayloadContentActions = viewModel.PayloadContentActions.OrderBy(x => x.Text).ToList();

        viewModel.PayloadContentActions.Insert(0, 
            new SelectListItem() {
                Text = "Ignore",
                Value = "Ignore"
            }
        );

        foreach (var file in request?.PayloadContents!)
        {
            if (file.JsonContent is not null)
            {
                string contentActionType = "Ignore";
                
                if (file.PayloadContentActions?.FirstOrDefault()?.Process == true)
                {
                    contentActionType = file.PayloadContentActions?.FirstOrDefault()?.PayloadContentActionType!;
                } 

                // Get mapping if exists
                var activeMappingId = file.PayloadContentActions?.Where(x => x.PayloadContentActionType == contentActionType).Select(x => x.ActiveMappingId).FirstOrDefault();
                Mapping? mapping = null;
                if (activeMappingId != null)
                {
                    mapping = await _mappingRepository.GetByIdAsync(activeMappingId.Value);
                }

                var test = new RequestManifestViewModel() {
                    timeZoneInfo = currentUserHelper.ResolvedCurrentUserTimeZone(),
                    PayloadContentId = file.Id,
                    Action = file.PayloadContentActions?.FirstOrDefault(),
                    ReceivedDate = file.CreatedAt,
                    FileName = file.FileName!,
                    ContentCategory = (file.XmlContent is not null || file.JsonContent is not null) ? "Data" : "File",
                    ContentType = file.ContentType!,
                    ReceviedCount = mapping?.ReceviedCount,
                    MappedCount = mapping?.MappedCount,
                    IgnoredCount = mapping?.IgnoredCount, 
                    PayloadContentActionType = contentActionType
                };
                viewModel.PayloadContents.Add(test);
            }
            
            
        }

        return View(viewModel);
    }

    [Route("/Preparing/{id:guid}")]
    [HttpPost]
    public async Task<IActionResult> Update(Guid id, CreateRequestManifestViewModel PayloadContent)
    {
        if (PayloadContent.Items is not null && PayloadContent.Items.Any())
        {
            foreach(var item in PayloadContent.Items)
            {
                if (item.PayloadContentId is null) break;
                
                var payloadContent = await _payloadContentRepository.GetByIdAsync(item.PayloadContentId.Value);

                if (payloadContent is null) break;
                if (item.Action is null) break;

                // Check if action exists
                var action = await _actionRepository.FirstOrDefaultAsync(new ActionByPayloadContentActionType(item.PayloadContentId.Value, item.OriginalAction));

                if (action is null && item.Action != "Ignore")
                {
                    // Create Action
                    var newAction = new PayloadContentAction()
                    {
                        PayloadContentId = item.PayloadContentId,
                        PayloadContentActionType = item.Action,
                        Process = true
                    };
                    await _actionRepository.AddAsync(newAction);
                }
                else if (action is not null)
                {
                    // Action exists, update it
                    action.Process = (item.Action == "Ignore") ? false : true;
                    await _actionRepository.UpdateAsync(action);
                }

                TempData[VoiceTone.Positive] = $"Updated payload actions.";
            }
        }
        
        return RedirectToAction(nameof(Index), new { id = id });
    }
    /*
    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportRequest(Guid id)
    {
        var incomingRequest = await _requestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(id));

        Guard.Against.Null(incomingRequest);

        // Create job
        var createdJob = await _jobService.CreateJobAsync(typeof(ImportRequestMappingsJob), typeof(Request), id);

        incomingRequest.RequestStatus = RequestStatus.WaitingToImport;

        await _requestRepository.UpdateAsync(incomingRequest);

        TempData[VoiceTone.Positive] = $"Request waiting to import ({incomingRequest.Id}).";
        return RedirectToAction(nameof(Index), new { id = id, jobId = createdJob.Id });
    }
    */
}