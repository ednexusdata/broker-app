using System.Threading.Tasks;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Web.ViewModels.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public class SystemController : AuthenticatedController<SystemController>
{
    private readonly ILogger<SystemController> logger;
    private readonly IMemoryCache cache;
    private readonly SettingsService settingsService;

    public SystemController(
        ILogger<SystemController> logger,
        IMemoryCache cache,
        SettingsService settingsService)
    {
        this.logger = logger;
        this.cache = cache;
        this.settingsService = settingsService;
    }

    public IActionResult Index()
    {
        var cacheStats = cache.GetCurrentStatistics();

        return View(new IndexViewModel
        {
            CacheStatistics = cacheStats
        });
    }

    [HttpDelete]
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
}