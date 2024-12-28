using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EdNexusData.Broker.Core.Cache;

public class MappingLookupCache
{
    private readonly ILogger<MappingLookupCache> _logger;

    private Dictionary<string, List<SelectListItem>> _cachedLookups = new Dictionary<string, List<SelectListItem>>();

    public MappingLookupCache(ILogger<MappingLookupCache> logger)
    {
        _logger = logger;
    }

    public List<SelectListItem>? Get(string cacheKey)
    {
        _logger.LogInformation($"Checking for key in mapping lookup cache: {cacheKey}");
        if (_cachedLookups.ContainsKey(cacheKey))  
        {  
            return Clone(_cachedLookups[cacheKey]);
        }  
        return null;
    }

    public void Add(string cacheKey, List<SelectListItem> selectList)
    {
        _logger.LogInformation($"Added key in mapping lookup cache: {cacheKey}");
        _cachedLookups.Add(cacheKey, Clone(selectList));
    }

    private List<SelectListItem> Clone(List<SelectListItem> original)
    {
        var returnSelectList = new List<SelectListItem>();

        foreach(var select in original)
        {
            returnSelectList.Add(new SelectListItem()
            {
                Text = select.Text,
                Value = select.Value,
                Selected = false
            });
        }
        return returnSelectList;
    }
}   