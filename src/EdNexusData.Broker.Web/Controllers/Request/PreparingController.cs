using System.ComponentModel;
using System.Text.Json;
using Ardalis.Specification;
using EdNexusData.Broker.Common.Connector;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Interfaces;
using EdNexusData.Broker.Core.Jobs;
using EdNexusData.Broker.Core.Serializers;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Worker;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.ViewModels.Preparing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferIncomingRecords)]
public class PreparingController : AuthenticatedController<RequestsController>
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<PayloadContentAction> _actionRepository;
    private readonly IRepository<Mapping> _mappingRepository;
    private readonly JobService _jobService;
    private readonly CurrentUserHelper currentUserHelper;
    private readonly EducationOrganizationContactService educationOrganizationContactService;
    private readonly ICurrentUser currentUser;
    private readonly INowWrapper nowWrapper;
    private readonly JobStatusService<PreparingController> jobStatusService;
    private readonly IRepository<EducationOrganizationConnectorSettings> edOrgConnectorSettingsRepo;
    private readonly ConnectorLoader _connectorLoader;

    public PreparingController(
        IRepository<Request> requestRepository, 
        IRepository<PayloadContent> payloadContentRepository, 
        IRepository<PayloadContentAction> actionRepository,
        IRepository<Mapping> mappingRepository,
        ConnectorLoader connectorLoader,
        JobService jobService,
        CurrentUserHelper currentUserHelper,
        EducationOrganizationContactService educationOrganizationContactService,
        ICurrentUser currentUser,
        INowWrapper nowWrapper,
        JobStatusService<PreparingController> jobStatusService,
        IRepository<EducationOrganizationConnectorSettings> edOrgConnectorSettingsRepo)
    {
        _requestRepository = requestRepository;
        _payloadContentRepository = payloadContentRepository;
        _actionRepository = actionRepository;
        _connectorLoader = connectorLoader;
        _mappingRepository = mappingRepository;
        _actionRepository = actionRepository;
        _jobService = jobService;
        this.currentUserHelper = currentUserHelper;
        this.educationOrganizationContactService = educationOrganizationContactService;
        this.currentUser = currentUser;
        this.nowWrapper = nowWrapper;
        this.jobStatusService = jobStatusService;
        this.edOrgConnectorSettingsRepo = edOrgConnectorSettingsRepo;
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

        if (request.RequestStatus.NotIn(RequestStatus.InProgress, RequestStatus.Reopened))
        {
            ViewBag.JobId = jobId;
        }

        var viewModel = new RequestManifestListViewModel
        {
            RequestId = id,
            RequestStatus = request.RequestStatus,
            Open = request.Open
        };

        if (request.PayloadContents is not null)
        {
            
            foreach (var file in request.PayloadContents)
            {
                string contentActionType = "Ignore";
                var resolvedPayloadContentActions = new List<SelectListItem>();
                    
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

                // Determine payload content actions that can respond to this payload content
                foreach (var contentPayloadAction in _connectorLoader.GetPayloadContentActions()!)
                {
                    var connectorNameForPayloadAction = contentPayloadAction.Assembly.GetName().Name;

                    // If connector is enabled
                    var enabledConnectors = await edOrgConnectorSettingsRepo.ListAsync(new EnabledConnectorsByEdOrgSpec(request.EducationOrganization!.ParentOrganizationId!.Value));
                    if (enabledConnectors.Where(x => x.Connector == connectorNameForPayloadAction).Count() == 0)
                    {
                        continue;
                    }
                    
                    // and if the connector has a transformer that responds to this payload content
                    if (file.ContentType == "application/json" && file.JsonContent is not null)
                    {
                        var payloadContentObject = DataPayloadContentSerializer.Deseralize(file.JsonContent.ToJsonString()!);
                        var payloadContentSchema = payloadContentObject.Schema;
                    
                        var transformerType = _connectorLoader.Transformers.Where(x => 
                            x.Key == $"{contentPayloadAction.Assembly.GetName().Name}::{payloadContentSchema?.Schema}::{payloadContentSchema?.SchemaVersion}").ToDictionary();

                        if (transformerType.Count > 0)
                        {
                            var connectorType = contentPayloadAction.Assembly.GetExportedTypes().Where(p => p.IsAssignableTo(typeof(IConnector))).FirstOrDefault();
                            
                            var displayNameType = connectorType?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault();
                            var displayName = "";
                            if (displayNameType is not null)
                            {
                                displayName = ((DisplayNameAttribute)displayNameType).DisplayName;
                            }

                            var contentPayloadActionDisplayNameType = contentPayloadAction?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault();
                            var contentPayloadActionDisplayName = "";
                            if (contentPayloadActionDisplayNameType is not null)
                            {
                                contentPayloadActionDisplayName = ((DisplayNameAttribute)contentPayloadActionDisplayNameType).DisplayName;
                            }

                            resolvedPayloadContentActions.Add(
                                new SelectListItem() {
                                    Text = $"{displayName} / {contentPayloadActionDisplayName}",
                                    Value = contentPayloadAction?.FullName
                                }
                            );
                        }
                    }
                }

                resolvedPayloadContentActions = resolvedPayloadContentActions.OrderBy(x => x.Text).ToList();

                resolvedPayloadContentActions.Insert(0, 
                    new SelectListItem() {
                        Text = "Ignore",
                        Value = "Ignore"
                    }
                );
                
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
                    PayloadContentActionType = contentActionType,
                    PayloadContentActions = resolvedPayloadContentActions
                };
                viewModel.PayloadContents.Add(test);
            }
            viewModel.PayloadContents = viewModel.PayloadContents.OrderBy(x => x.ContentCategory).ThenBy(x => x.FileName).ToList();
        }

        return View(viewModel);
    }

    [Route("/Preparing/{id:guid}")]
    [HttpPost]
    public async Task<IActionResult> Update(Guid id, CreateRequestManifestViewModel PayloadContent)
    {
        if (PayloadContent.Items is not null && PayloadContent.Items.Any())
        {
            PayloadContent? payloadContent = null;
            
            foreach(var item in PayloadContent.Items)
            {
                if (item.PayloadContentId is null) break;
                
                payloadContent = await _payloadContentRepository.GetByIdAsync(item.PayloadContentId.Value);

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
            }

            // Get request
            var request = await _requestRepository.GetByIdAsync(id);

            if (payloadContent is not null && request is not null && request.RequestStatus != RequestStatus.InProgress)
            {
                TempData[VoiceTone.Positive] = $"Updated payload actions.";
                // Queue job to send update
                var jobData = new MessageContents { 
                    Sender = await educationOrganizationContactService.FromUser(currentUserHelper.CurrentUserId()!.Value), 
                    SenderSentTimestamp = nowWrapper.UtcNow, 
                    RequestId = payloadContent.RequestId, 
                    RequestStatus = RequestStatus.InProgress, 
                    EducationOrganizationId = request.EducationOrganizationId,
                    MessageText = "Updated request status to in progress."
                };
                var job = await _jobService.CreateJobAsync(
                    typeof(SendMessageJob), 
                    typeof(Request), 
                    payloadContent.RequestId, 
                    currentUserHelper.CurrentUserId()!.Value, 
                    JsonSerializer.SerializeToDocument(jobData)
                );

                await jobStatusService.UpdateRequestStatus(request, RequestStatus.InProgress, "Started setting payload content actions");
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