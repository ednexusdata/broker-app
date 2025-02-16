using System.ComponentModel;

namespace EdNexusData.Broker.Web.Helpers;

public class ConnectorHelper
{
    public string DisplayName(Type connector)
    {
        return ((DisplayNameAttribute)connector
                        .GetCustomAttributes(false)
                        .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName
            ;
    }
}