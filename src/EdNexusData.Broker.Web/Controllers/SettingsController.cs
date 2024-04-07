using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Connector;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Service;
using EdNexusData.Broker.Service.Serializers;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.Models;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public partial class SettingsController : AuthenticatedController<SettingsController>
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConfigurationSerializer _configurationSerializer;
    private readonly IncomingPayloadSerializer _incomingPayloadSerializer;
    private readonly OutgoingPayloadSerializer _outgoingPayloadSerializer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<EducationOrganizationConnectorSettings> _repo;
    private readonly IRepository<EducationOrganizationPayloadSettings> _educationOrganizationPayloadSettings;
    private readonly PayloadContentTypeService _payloadContentTypeService;
    
    private readonly FocusHelper _focusHelper;

    private Guid? _focusedDistrictEdOrg { get; set; }

    public SettingsController(
        ConnectorLoader connectorLoader, 
        IServiceProvider serviceProvider, 
        IRepository<EducationOrganizationConnectorSettings> repo, 
        FocusHelper focusHelper, 
        ConfigurationSerializer configurationSerializer, 
        IRepository<EducationOrganizationPayloadSettings> educationOrganizationPayloadSettings,
        IncomingPayloadSerializer incomingPayloadSerializer,
        OutgoingPayloadSerializer outgoingPayloadSerializer,
        PayloadContentTypeService payloadContentTypeService
        )
    {
        ArgumentNullException.ThrowIfNull(connectorLoader);

        _configurationSerializer = configurationSerializer;
        _connectorLoader = connectorLoader;
        _serviceProvider = serviceProvider;
        _repo = repo;
        _focusHelper = focusHelper;
        _educationOrganizationPayloadSettings = educationOrganizationPayloadSettings;
        _incomingPayloadSerializer = incomingPayloadSerializer;
        _outgoingPayloadSerializer = outgoingPayloadSerializer;
        _payloadContentTypeService = payloadContentTypeService;
    }

    public async Task<IActionResult> Index()
    {
        if (await FocusedToDistrict() is not null) return View();
        
        var connectors = _connectorLoader.Connectors;
        var payloads = _connectorLoader.Payloads;

        var settingsViewModel = new SettingsViewModel() {
            ConnectorTypes = connectors,
            PayloadTypes = payloads
        };

        return View(settingsViewModel);
    }

    [HttpGet("/Settings/Configuration/{assembly}")]
    public async Task<IActionResult> Configuration(string assembly)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict()!;

        var connectorDictionary = _connectorLoader.Assemblies.Where(x => x.Key == assembly).FirstOrDefault();
        ArgumentException.ThrowIfNullOrEmpty(assembly);
        var connector = connectorDictionary.Value;

        // Get configurations for connector - TO FIX!
        var configurations = _connectorLoader.GetConfigurations(connector);

        var forms = new List<dynamic>();

        if (configurations is not null)
        {
            foreach(var configType in configurations)
            {
                var configModel = await _configurationSerializer.DeseralizeAsync(configType, _focusedDistrictEdOrg!.Value);
                var displayName = (DisplayNameAttribute)configType.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!;

                forms.Add(
                    new { 
                        displayName = displayName.DisplayName, 
                        html = ModelFormBuilderHelper.HtmlForModel(configModel) 
                    }
                );
            }
        }
        
        return View(forms);
    }

    [HttpPost("/Settings/Configuration/{assembly}")]
    public async Task<IActionResult> Update(IFormCollection collection)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict()!;

        var assemblyQualifiedName = collection["ConnectorConfigurationType"];

        // Get Connector Config Type
        Type connectorConfigType = Type.GetType(assemblyQualifiedName!, true)!;

        var iconfigModel = await _configurationSerializer.DeseralizeAsync(connectorConfigType, _focusedDistrictEdOrg!.Value);

        // Loop through properties and set from form
        foreach(var prop in iconfigModel.GetType().GetProperties())
        {
            prop.SetValue(iconfigModel, collection[prop.Name].ToString());
        }

        await _configurationSerializer.SerializeAndSaveAsync(iconfigModel, _focusedDistrictEdOrg!.Value);

        TempData[VoiceTone.Positive] = $"Updated Settings.";

        return RedirectToAction("Configuration", new { assembly = connectorConfigType.Assembly.GetName().Name });
    }

    [HttpGet("/Settings/Mapping")]
    public async Task<IActionResult> Mapping()
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict()!;
        
        return View(new {});
    }

    private async Task<IActionResult?> FocusedToDistrict()
    {
        if (_focusedDistrictEdOrg == null)
        {
            _focusedDistrictEdOrg = await _focusHelper.CurrentDistrictEdOrgFocus();
        }
        
        if (!_focusedDistrictEdOrg.HasValue)
        {
            TempData[VoiceTone.Critical] = $"Must be focused to a district.";
            return RedirectToAction(nameof(Index));
        }

        return null;
    }
}
