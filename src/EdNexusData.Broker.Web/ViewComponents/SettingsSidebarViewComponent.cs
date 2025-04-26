using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Models;
using EdNexusData.Broker.Web.Helpers;

namespace EdNexusData.Broker.Web.ViewComponents;

public class SettingsSidebarViewComponent : ViewComponent
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly FocusHelper focusHelper;
    private readonly IRepository<EducationOrganizationConnectorSettings> repo;

    public SettingsSidebarViewComponent(
        ConnectorLoader connectorLoader, 
        FocusHelper focusHelper,
        IRepository<EducationOrganizationConnectorSettings> repo
    )
    {
        _connectorLoader = connectorLoader;
        this.focusHelper = focusHelper;
        this.repo = repo;
    }

    public async Task<IViewComponentResult> InvokeAsync(string selectedView = "")
    {
        var focusedDistrictEdOrg = await focusHelper.CurrentDistrictEdOrgFocus();

        var viewModel = new SettingsSidebarViewModel();
        
        var connectors = _connectorLoader.Connectors;

        var connectorSettings = await repo.ListAsync(new ConnectorByEdOrgIdSpec(focusedDistrictEdOrg!.Value));

        foreach(var connector in connectors)
        {   
            if (connector is not null && connectorSettings.Any(x => x.Connector == connector.Assembly.GetName().Name && x.Enabled == true))
            {
                viewModel.Connectors.Add(new SettingsSidebarViewModel.ConnectorSidebarViewModel() {
                    DisplayName = ((DisplayNameAttribute)connector.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!).DisplayName,
                    ConnectorTypeName = connector.Assembly.GetName().Name!,
                    Selected = (selectedView == connector.Assembly.GetName().Name) ? true : false
                });
            }
            
        }

        var payloads = _connectorLoader.Payloads;

        foreach(var payload in payloads)
        {   
            if (payload is not null)
            {
                viewModel.Payloads.Add(new SettingsSidebarViewModel.PayloadSidebarViewModel() {
                    DisplayName = ((DisplayNameAttribute)payload.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!).DisplayName,
                    PayloadTypeName = payload.FullName!,
                    Selected = (selectedView == payload.FullName) ? true : false
                });
            }
            
        }

        return View(viewModel);
    }

}