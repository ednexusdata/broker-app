using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EdNexusData.Broker.Web.Helpers;

public class TimezoneHelper
{
    private readonly CurrentUserHelper currentUserHelper;

    public TimezoneHelper(CurrentUserHelper currentUserHelper)
    {
        this.currentUserHelper = currentUserHelper;
    }

    public string? DisplayTimeFromUtc(DateTime datetime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(datetime, currentUserHelper.ResolvedCurrentUserTimeZone()).ToString("M/dd/yyyy h:mm tt");
    }
    
    public string? DisplayTimeFromUtc(DateTimeOffset? datetimeOffset)
    {
        if (datetimeOffset is null) return null;
        return TimeZoneInfo.ConvertTimeFromUtc(datetimeOffset.Value.DateTime, currentUserHelper.ResolvedCurrentUserTimeZone()).ToString("M/dd/yyyy h:mm tt");
    }

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