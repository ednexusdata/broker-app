using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Frozen;

namespace EdNexusData.Broker.Core.Cache;

public class MappingLookupCache
{
    private readonly ILogger<MappingLookupCache> _logger;

    private Dictionary<string, List<SelectListItem>> _cachedLookups = new Dictionary<string, List<SelectListItem>>();

    private FrozenDictionary<string, List<SelectListItem>>? frozenCachedLookups;

    public MappingLookupCache(ILogger<MappingLookupCache> logger)
    {
        _logger = logger;
    }

    public List<SelectListItem>? Get(string cacheKey)
    {
        _logger.LogInformation($"Checking for key in mapping lookup cache: {cacheKey}");
        if (frozenCachedLookups is not null && frozenCachedLookups.TryGetValue(cacheKey, out var value))  
        {  
            _logger.LogInformation($"Cache key found for: {cacheKey}");
            return Clone(value);
        }  
        return null;
    }

    public void Add(string cacheKey, List<SelectListItem> selectList)
    {
        _logger.LogInformation($"Added key in mapping lookup cache: {cacheKey}");
        _cachedLookups.Add(cacheKey, Clone(selectList));
        frozenCachedLookups = _cachedLookups.ToFrozenDictionary();
    }

    private List<SelectListItem> Clone(List<SelectListItem> original)
    {
        var returnSelectList = new List<SelectListItem>();

        SelectListGroup? lastGroup = null;

        foreach(var select in original)
        {
            if ((lastGroup is null && select.Group is not null) || (lastGroup is not null && lastGroup.Name != select.Group?.Name))
            {
                lastGroup = new SelectListGroup()
                {
                    Name = select.Group?.Name
                };
            }

            returnSelectList.Add(new SelectListItem()
            {
                Group = lastGroup,
                Text = select.Text,
                Value = select.Value,
                Selected = false
            });
        }
        return returnSelectList;
    }
}   