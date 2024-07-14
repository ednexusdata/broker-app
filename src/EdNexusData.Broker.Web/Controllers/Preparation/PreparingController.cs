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
    private readonly ConnectorLoader _connectorLoader;

    public PreparingController(IRepository<Request> requestRepository, IRepository<Domain.PayloadContent> payloadContentRepository, ConnectorLoader connectorLoader)
    {
        _requestRepository = requestRepository;
        _payloadContentRepository = payloadContentRepository;
        _connectorLoader = connectorLoader;
    }

    [Route("/Preparing/{id:guid}")]
    public async Task<IActionResult> Index(Guid id)
    {
        // Get all payload content files for request
        var request = await _requestRepository.FirstOrDefaultAsync(new RequestByIdWithMessagesPayloadContents(id));

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

        foreach (var file in request?.PayloadContents!)
        {
            if (file.JsonContent is not null)
            {
                var test = new RequestManifestViewModel() {
                    PayloadContentId = file.Id,
                    PayloadContentAction = file.Actions!.FirstOrDefault()!,
                    ReceivedDate = file.CreatedAt,
                    FileName = file.FileName!,
                    ContentCategory = (file.XmlContent is not null || file.JsonContent is not null) ? "Data" : "File",
                    ContentType = file.ContentType!,
                    ReceviedCount = file.JsonContent!.RootElement.GetProperty("Content").EnumerateArray().Count() // json["Content"].AsJEnumerable().Count(),
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

                var payloadContentAction = new PayloadContentAction()
                {
                    ConnectorAction = item.PayloadContentAction!
                };

                payloadContent.Actions!.RemoveAll(action => true == true);
                payloadContent.Actions.Add(payloadContentAction);
                await _payloadContentRepository.UpdateAsync(payloadContent);

                TempData[VoiceTone.Positive] = $"Updated payload actions.";
            }
        }
        
        return RedirectToAction(nameof(Index), new { id = id });
    }
}