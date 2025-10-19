using Microsoft.Extensions.Caching.Memory;

namespace EdNexusData.Broker.Web.ViewModels.System;

public class IndexViewModel
{
  public MemoryCacheStatistics? CacheStatistics { get; set; }

  public List<Setting> Settings { get; set; } = new();
}