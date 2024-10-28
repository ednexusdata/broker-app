using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EdNexusData.Broker.Web.Helpers;

public class TimezoneHelper
{
    public static List<SelectListItem> TimezoneSelectList()
    {
        var selectListItems = new List<SelectListItem>();
        
        ReadOnlyCollection<TimeZoneInfo> tzCollection = TimeZoneInfo.GetSystemTimeZones();
        foreach(var tz in tzCollection)
        {
            selectListItems.Add(new SelectListItem()
            {
                Value = tz.Id,
                Text = tz.DisplayName
            });
        }

        return selectListItems;
    }
}