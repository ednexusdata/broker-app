using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Web.Constants.Claims;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.ViewModels.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = CustomClaimType.SuperAdmin)]
public class SystemController : AuthenticatedController<SystemController>
{
    private readonly ILogger<SystemController> logger;
    private readonly IMemoryCache cache;
    private readonly SettingsService settingsService;
    private readonly IRepository<Setting> settingsRepository;

    public SystemController(
        ILogger<SystemController> logger,
        IMemoryCache cache,
        SettingsService settingsService,
        IRepository<Setting> settingsRepository)
    {
        this.logger = logger;
        this.cache = cache;
        this.settingsService = settingsService;
        this.settingsRepository = settingsRepository;
    }

    public async Task<IActionResult> Index()
    {
        return View(new IndexViewModel
        {
            CacheStatistics = cache.GetCurrentStatistics(),
            Settings = await settingsRepository.ListAsync(new SettingsSortByKeySpecification())
        });
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCache()
    {
        if (cache is MemoryCache memCache)
        {
            memCache.Compact(1.0);
            logger.LogInformation("Cache cleared.");

            await settingsService.SetValueAsync("LastCacheClearedAt", DateTime.UtcNow.ToString("o"));
        }

        TempData[VoiceTone.Positive] = $"Cleared cache.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(List<Setting> settings)
    {
        var changeCount = 0;
        foreach (var setting in settings)
        {
            // Get existing setting
            var dbSetting = await settingsRepository.GetByIdAsync(setting.Id);
            if (dbSetting is not null && dbSetting.Value != setting.Value)
            {
                dbSetting.Value = setting.Value;
                await settingsRepository.UpdateAsync(dbSetting);
                changeCount++;
            }
        }

        TempData[VoiceTone.Positive] = $"Updated {changeCount} setting(s).";
        return RedirectToAction("Index");
    }
}