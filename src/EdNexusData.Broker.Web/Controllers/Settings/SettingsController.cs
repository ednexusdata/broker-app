using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Core;
using EdNexusData.Broker.Core.Serializers;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.Models;
using EdNexusData.Broker.Common;

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
    private readonly PayloadJobService _payloadJobService;
    
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
        PayloadJobService payloadJobService
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
        _payloadJobService = payloadJobService;
    }

    public async Task<IActionResult> Index()
    {
        if (await FocusedToDistrict() is not null) return View();
        
        var connectors = _connectorLoader.Connectors;
        var payloads = _connectorLoader.Payloads;

        var connectorSettings = await _repo.ListAsync(new ConnectorByEdOrgIdSpec(_focusedDistrictEdOrg!.Value));

        var settingsViewModel = new SettingsViewModel() {
            ConnectorTypes = connectors,
            PayloadTypes = payloads,
            ConnectorSettings = connectorSettings,
        };

        return View(settingsViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Update(IEnumerable<string> connectorsEnabled)
    {
        if (await FocusedToDistrict() is not null) return View();

        var connectors = _connectorLoader.Connectors;

        if (connectorsEnabled is not null && connectorsEnabled.Count() > 0)
        {
            foreach(var connector in connectors)
            {
                var existingRecord = await _repo.FirstOrDefaultAsync(new ConnectorByNameAndEdOrgIdSpec(connector.Assembly.GetName().Name!, _focusedDistrictEdOrg!.Value));
                if (existingRecord is not null)
                {
                    existingRecord.Enabled = connectorsEnabled.Contains(connector.Assembly.GetName().Name!);
                    await _repo.UpdateAsync(existingRecord);
                }
                else
                {
                    if (connectorsEnabled.Contains(connector.Assembly.GetName().Name!))
                    {
                        var newRecord = new EducationOrganizationConnectorSettings()
                        {
                            Connector = connector.Assembly.GetName().Name!,
                            EducationOrganizationId = _focusedDistrictEdOrg!.Value,
                            Enabled = connectorsEnabled.Contains(connector.Assembly.GetName().Name!)
                        };
                        await _repo.AddAsync(newRecord);
                    }
                    
                }
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/Settings/Configuration/{assembly}")]
    public async Task<IActionResult> Configuration(string assembly)
    {
        var result = await FocusedToDistrict();
        if (result != null) return result;

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
    public async Task<IActionResult> UpdateConfiguration(IFormCollection collection)
    {
        var result = await FocusedToDistrict();
        if (result != null) return result;

        var assemblyQualifiedName = collection["ConnectorConfigurationType"];

        // Get Connector Config Type
        Type connectorConfigType = Type.GetType(assemblyQualifiedName!, true)!;

        var iconfigModel = await _configurationSerializer.DeseralizeAsync(connectorConfigType, _focusedDistrictEdOrg!.Value);

        // Loop through properties and set from form
        foreach(var prop in iconfigModel.GetType().GetProperties())
        {
            if (collection[prop.Name].ToString() != "ValueSet")
            {
                prop.SetValue(iconfigModel, collection[prop.Name].ToString());
            }
        }

        await _configurationSerializer.SerializeAndSaveAsync(iconfigModel, _focusedDistrictEdOrg!.Value);

        TempData[VoiceTone.Positive] = $"Updated Settings.";

        return RedirectToAction("Configuration", new { assembly = connectorConfigType.Assembly.GetName().Name });
    }

    [HttpGet("/Settings/Mapping")]
    public async Task<IActionResult> Mapping()
    {
        var result = await FocusedToDistrict();
        if (result != null) return result;
        
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
