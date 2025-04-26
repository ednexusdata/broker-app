using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.Helpers;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public partial class SettingsController : AuthenticatedController<SettingsController>
{
    [HttpGet("/Settings/IncomingPayload/{payload}")]
    public async Task<IActionResult> IncomingPayload(string payload)
    {
        var result = await FocusedToDistrict();
        if (result != null) return result;

        var payloadAssembly = _connectorLoader.Payloads.Where(x => x.FullName == payload).First();

        var currentPayload = await _educationOrganizationPayloadSettings
            .FirstOrDefaultAsync(new PayloadSettingsByNameAndEdOrgIdSpec(payload, _focusedDistrictEdOrg!.Value));           

        return View(
            new { Payload = 
                new { 
                    FullName = payload,
                    ((DisplayNameAttribute)payloadAssembly
                        .GetCustomAttributes(false)
                        .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName
                },
                ConnectorListItems = ConnectorSelectMenuHelper.ConnectorsListMenu(
                    _connectorLoader.Connectors, 
                    currentPayload?.IncomingPayloadSettings?.StudentInformationSystem
                )
            }
        );
    }

    [HttpPost("/Settings/IncomingPayload/{payload}")]
    public async Task<IActionResult> UpdateIncomingPayload([FromRoute] string payload)
    {
        var result = await FocusedToDistrict();
        if (result != null) return result;
        
        var currentPayload = await _educationOrganizationPayloadSettings
            .FirstOrDefaultAsync(new PayloadSettingsByNameAndEdOrgIdSpec(payload, _focusedDistrictEdOrg!.Value));

        if (currentPayload is not null)
        {
            if (currentPayload.IncomingPayloadSettings is not null)
            {
                currentPayload.IncomingPayloadSettings!.StudentInformationSystem = Request.Form.Where(i => i.Key == "StudentInformationSystem").FirstOrDefault().Value.ToString();
            }
            else
            {
                currentPayload.IncomingPayloadSettings = new IncomingPayloadSettings()
                {
                    StudentInformationSystem = Request.Form.Where(i => i.Key == "StudentInformationSystem").FirstOrDefault().Value.ToString()
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
                IncomingPayloadSettings = new IncomingPayloadSettings()
                {
                    StudentInformationSystem = Request.Form.Where(i => i.Key == "StudentInformationSystem").FirstOrDefault().Value.ToString()
                }
            });
        }

        TempData[VoiceTone.Positive] = $"Updated Incoming Payload.";

        return RedirectToAction("IncomingPayload", new { payload });
    }
}