using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.ViewModels;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public class ConnectorsController : AuthenticatedController<ConnectorsController>
{
    private readonly ConnectorService connectorService;

    public ConnectorsController(ConnectorService connectorService)
    {
        this.connectorService = connectorService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var storeConnectors = await connectorService.GetStoreConnectors();

        var referenceConnectors = await connectorService.GetConnectorReferences();

        return View(new ConnectorsViewModel { ReferenceConnectors = referenceConnectors, StoreConnectors = storeConnectors });
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> Add(string connectorReference)
    {
        var connector = await connectorService.GetStoreConnector(connectorReference);
        _ = connector ?? throw new NullReferenceException("Unable to find connector from connector store");

        var savedConnector = await connectorService.AddConnectorReference(connector);

        TempData[VoiceTone.Positive] = $"Added connector {savedConnector.Reference} ({savedConnector.Id}).";
        return RedirectToAction(nameof(Index));
    }

    [ValidateAntiForgeryToken]
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid connectorReferenceId)
    {
        var connector = await connectorService.GetConnectorReference(connectorReferenceId);
        _ = connector ?? throw new NullReferenceException("Unable to find connector reference");

        await connectorService.RemoveConnectorReference(connector);

        TempData[VoiceTone.Positive] = $"Removed connector {connector.Reference} ({connector.Id}).";
        return RedirectToAction(nameof(Index));
    }
    
}
