using EdNexusData.Broker.Core.Models;

namespace EdNexusData.Broker.Web.ViewModels;

public class ConnectorsViewModel
{
  public List<ConnectorReference>? ReferenceConnectors { get; set; }
  public List<StoreConnector>? StoreConnectors { get; set; }
}