using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.Service;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.ViewModels.Settings;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public partial class SettingsController : AuthenticatedController<SettingsController>
{
    [HttpGet("/Settings/OutgoingPayload/{payload}")]
    public async Task<IActionResult> OutgoingPayload(string payload)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

        var payloadAssembly = _connectorLoader.Payloads.Where(x => x.FullName == payload).First();

        var currentPayload = await _educationOrganizationPayloadSettings
            .FirstOrDefaultAsync(new PayloadSettingsByNameAndEdOrgIdSpec(payload, _focusedDistrictEdOrg!.Value));

        var contentTypes = _payloadContentTypeService.GetPayloadContentTypes().OrderBy(i => i.DisplayName) ?? Enumerable.Empty<PayloadContentTypeDisplay>();

        // Format for json on screen
        var settings = new List<PayloadSettingsViewModel>();
        if (currentPayload is not null 
            && currentPayload?.OutgoingPayloadSettings is not null 
            && currentPayload?.OutgoingPayloadSettings.PayloadContents is not null
            && currentPayload?.OutgoingPayloadSettings.PayloadContents?.Count() > 0
        )
        {
            foreach(var currentSettings in currentPayload.OutgoingPayloadSettings.PayloadContents)
            {
                settings.Add(new PayloadSettingsViewModel() {
                    FullName = currentSettings.PayloadContentType,
                    DisplayName = contentTypes.Where(a => a.FullName == currentSettings.PayloadContentType).FirstOrDefault()!.DisplayName,
                    Configuration = currentSettings.Settings
                });
            }
        }
        settings = settings.OrderBy(i => i.DisplayName).ToList();

        return View(new
        {
            ContentTypes = contentTypes,
            Payload = new 
            {
                FullName = payload,
                ((DisplayNameAttribute)payloadAssembly
                    .GetCustomAttributes(false)
                    .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName,
                Settings = settings
            },
            ConnectorListItems = ConnectorSelectMenuHelper.ConnectorsListMenu(
                _connectorLoader.Connectors, 
                currentPayload?.OutgoingPayloadSettings?.StudentLookupConnector
            )
        });
    }

    [HttpPost("/Settings/OutgoingPayload/{payload}")]
    public async Task<IActionResult> UpdateOutgoingPayload([FromRoute] string payload, [FromForm] CreateOutgoingPayloadSettingsViewModel input)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

        var currentPayload = await _educationOrganizationPayloadSettings
            .FirstOrDefaultAsync(new PayloadSettingsByNameAndEdOrgIdSpec(payload, _focusedDistrictEdOrg!.Value));

        if (currentPayload is not null && currentPayload.OutgoingPayloadSettings is not null)
        {
            currentPayload.OutgoingPayloadSettings.StudentLookupConnector = input.StudentLookupConnector;
            await _educationOrganizationPayloadSettings.UpdateAsync(currentPayload);
        }
        else
        {
            await _educationOrganizationPayloadSettings.AddAsync(new EducationOrganizationPayloadSettings()
            {
                EducationOrganizationId = _focusedDistrictEdOrg!.Value,
                Payload = payload,
                OutgoingPayloadSettings = new OutgoingPayloadSettings()
                {
                    StudentLookupConnector = input.StudentLookupConnector
                }
            });
        }

        TempData[VoiceTone.Positive] = $"Updated Outgoing Payload.";

        return RedirectToAction("OutgoingPayload", new { payload });
    }

    [HttpPost("/Settings/OutgoingPayloadContents/{payload}")]
    public async Task<IActionResult> UpdateOutgoingPayloadContents([FromRoute] string payload, [FromForm] string settings)
    {
        if (await FocusedToDistrict() is not null) return await FocusedToDistrict();

        // Transform incoming form json data to jsonnode
        var jsonSettings = JsonNode.Parse(settings)!.AsArray();

        // Start Payload Settings Content Types
        var payloadContentTypeSettings = new List<PayloadSettingsContentType>();
        
        foreach(var jsonSetting in jsonSettings)
        {
            payloadContentTypeSettings.Add(
                new PayloadSettingsContentType()
                {
                    PayloadContentType = jsonSetting["fullName"].ToString(),
                    Settings = jsonSetting["configuration"].ToString()
                }
            );
        }

        var currentPayload = await _educationOrganizationPayloadSettings
            .FirstOrDefaultAsync(new PayloadSettingsByNameAndEdOrgIdSpec(payload, _focusedDistrictEdOrg!.Value));

        if (currentPayload is not null)
        {
            if (currentPayload.OutgoingPayloadSettings is not null)
            {
                currentPayload.OutgoingPayloadSettings.PayloadContents = payloadContentTypeSettings;
            }
            else
            {
                currentPayload.OutgoingPayloadSettings = new OutgoingPayloadSettings()
                {
                    PayloadContents = payloadContentTypeSettings
                };
            }
            
            await _educationOrganizationPayloadSettings.UpdateAsync(currentPayload);
        }
        else
        {
            await _educationOrganizationPayloadSettings.AddAsync(new EducationOrganizationPayloadSettings()
            {
                EducationOrganizationId = _focusedDistrictEdOrg!.Value,
                Payload = payload,
                OutgoingPayloadSettings = new OutgoingPayloadSettings()
                {
                    PayloadContents = payloadContentTypeSettings
                }
            });
        }

        TempData[VoiceTone.Positive] = $"Updated Outgoing Payload.";

        return RedirectToAction("OutgoingPayload", new { payload });
    }
}