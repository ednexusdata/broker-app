using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Core.Resolvers;
using EdNexusData.Broker.Core.Cache;
using EdNexusData.Broker.Common.Lookup;

namespace EdNexusData.Broker.Core.Lookup;

public class MappingLookupService
{
    private readonly ILogger<MappingLookupService> _logger;
    private readonly MappingLookupResolver _mappingLookupResolver;
    private readonly MappingLookupCache _mappingLookupCache;
    private readonly FocusEducationOrganizationResolver focusEducationOrganizationResolver;

  public MappingLookupService(ILogger<MappingLookupService> logger, 
        MappingLookupResolver mappingLookupResolver,
        MappingLookupCache mappingLookupCache,
        FocusEducationOrganizationResolver focusEducationOrganizationResolver)
    {
        _logger = logger;
        _mappingLookupResolver = mappingLookupResolver;
        _mappingLookupCache = mappingLookupCache;
        this.focusEducationOrganizationResolver = focusEducationOrganizationResolver;
  }

    public async Task<List<SelectListItem>> SelectAsync(LookupAttribute lookupAttribute, string? value, string? keyprefix = null)
    {
        if (keyprefix is null)
        {
            keyprefix = (await focusEducationOrganizationResolver.Resolve()).Id.ToString();
        }
        
        var selectList = _mappingLookupCache.Get($"{keyprefix}::{lookupAttribute.LookupType.Name}");
        
        // Determine if lookup already called and loaded
        if (selectList is null)
        { 
            _logger.LogInformation($"{keyprefix}::{lookupAttribute.LookupType.Name} not found in cache. Fetching...");
            // Resolve lookup to call
            var mappingLookupObj = _mappingLookupResolver.Resolve(lookupAttribute.LookupType);
            selectList = await mappingLookupObj.SelectListAsync();

            // Cache the value
            _mappingLookupCache.Add($"{keyprefix}::{lookupAttribute.LookupType.Name}", selectList);
        }
        
        // Set the selected value
        if (value is not null)
        {
            var selected = selectList.FindIndex(x => x.Value == value);
            if (selected > -1)
            {
                selectList[selected].Selected = true;
            }
        }

        return selectList;
    }

    public async Task<string?> GetAsync(LookupAttribute lookupAttribute, string? value, string? keyprefix = null)
    {
        if (keyprefix is null)
        {
            keyprefix = (await focusEducationOrganizationResolver.Resolve()).Id.ToString();
        }
        
        var selectList = _mappingLookupCache.Get($"{keyprefix}::{lookupAttribute.LookupType.Name}");
        
        // Determine if lookup already called and loaded
        if (selectList is null)
        {
            _logger.LogInformation($"{keyprefix}::{lookupAttribute.LookupType.Name} not found in cache. Fetching...");
            // Resolve lookup to call
            var mappingLookupObj = _mappingLookupResolver.Resolve(lookupAttribute.LookupType);
            selectList = await mappingLookupObj.SelectListAsync();

            // Cache the value
            _mappingLookupCache.Add($"{keyprefix}::{lookupAttribute.LookupType.Name}", selectList);
        }
        
        // Set the selected value
        var selected = selectList.Where(x => x.Value == value).FirstOrDefault();

        return selected?.Text ?? null;
    }
}   