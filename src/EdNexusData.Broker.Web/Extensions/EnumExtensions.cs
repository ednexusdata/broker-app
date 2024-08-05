namespace EdNexusData.Broker.Web;

public static class EnumExtensions
{
    public static bool In<T>(this T val, params T[] values) where T : struct
    {
        return values.Contains(val);
    }

    public static bool NotIn<T>(this T val, params T[] values) where T : struct
    {
        return ! values.Contains(val);
    }
}