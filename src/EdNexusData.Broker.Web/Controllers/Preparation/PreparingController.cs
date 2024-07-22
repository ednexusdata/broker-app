using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Connector;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Extensions;
using EdNexusData.Broker.Web.ViewModels.Preparing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferOutgoingRecords)]
public class PreparingController : AuthenticatedController<RequestsController>
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Domain.PayloadContent> _payloadContentRepository;
    private readonly IRepository<PayloadContentAction> _actionRepository;
    private readonly ConnectorLoader _connectorLoader;

    public PreparingController(
        IRepository<Request> requestRepository, 
        IRepository<Domain.PayloadContent> payloadContentRepository, 
        IRepository<PayloadContentAction> actionRepository,
        ConnectorLoader connectorLoader)
    {
        _requestRepository = requestRepository;
        _payloadContentRepository = payloadContentRepository;
        _connectorLoader = connectorLoader;
        _actionRepository = actionRepository;
    }

    [Route("/Preparing/{id:guid}")]
    public async Task<IActionResult> Index(Guid id)
    {
        // Get all payload content files for request
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdWithMessagesPayloadContents(id));
        if (request == null)
        {
            return NotFound();
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

                var test = new RequestManifestViewModel() {
                    PayloadContentId = file.Id,
                    Action = file.PayloadContentActions?.FirstOrDefault(),
                    ReceivedDate = file.CreatedAt,
                    FileName = file.FileName!,
                    ContentCategory = (file.XmlContent is not null || file.JsonContent is not null) ? "Data" : "File",
                    ContentType = file.ContentType!,
                    ReceviedCount = file.JsonContent!.RootElement.GetProperty("Content").EnumerateArray().Count(), // json["Content"].AsJEnumerable().Count(),
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
}