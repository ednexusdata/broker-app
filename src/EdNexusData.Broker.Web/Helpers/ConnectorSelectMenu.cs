using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using EdNexusData.Broker.Connector;

namespace EdNexusData.Broker.Web.Helpers;

public class ConnectorSelectMenuHelper
{
    public static List<SelectListItem> ConnectorsListMenu(List<Type> connectors, string? currentDataConnector)
    {
        // Create select menu
        var connectorListItems = new List<SelectListItem>();

        foreach(var connector in connectors)
        {
            connectorListItems.Add(new SelectListItem() {
                Text = ((DisplayNameAttribute)connector
                        .GetCustomAttributes(false)
                        .First(x => x.GetType() == typeof(DisplayNameAttribute))).DisplayName,
                Value = connector.FullName,
                Selected = (currentDataConnector is not null && connector.FullName == currentDataConnector) ? true : false
            });
        }
        connectorListItems = connectorListItems.OrderBy(x => x.Text).ToList();

        return connectorListItems;
    }
}