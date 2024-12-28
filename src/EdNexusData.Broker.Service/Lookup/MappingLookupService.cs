using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Service.Resolvers;
using EdNexusData.Broker.Service.Cache;
using EdNexusData.Broker.Common.Lookup;

namespace EdNexusData.Broker.Service.Lookup;

public class MappingLookupService
{
    private readonly ILogger<MappingLookupService> _logger;
    private readonly MappingLookupResolver _mappingLookupResolver;
    private readonly MappingLookupCache _mappingLookupCache;

    public MappingLookupService(ILogger<MappingLookupService> logger, 
        MappingLookupResolver mappingLookupResolver,
        MappingLookupCache mappingLookupCache)
    {
        _logger = logger;
        _mappingLookupResolver = mappingLookupResolver;
        _mappingLookupCache = mappingLookupCache;
    }

    public async Task<List<SelectListItem>> SelectAsync(LookupAttribute lookupAttribute, string? value)
    {
        var selectList = _mappingLookupCache.Get(lookupAttribute.LookupType.Name);
        
        // Determine if lookup already called and loaded
        if (selectList is null)
        {
            _logger.LogInformation($"{lookupAttribute.LookupType.Name} not found in cache. Fetching...");
            // Resolve lookup to call
            var mappingLookupObj = _mappingLookupResolver.Resolve(lookupAttribute.LookupType);
            selectList = await mappingLookupObj.SelectListAsync();

            // Cache the value
            _mappingLookupCache.Add(lookupAttribute.LookupType.Name, selectList);
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

    
}   