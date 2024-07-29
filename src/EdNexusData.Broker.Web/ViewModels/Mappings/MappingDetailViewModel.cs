using System.Reflection;

namespace EdNexusData.Broker.Web.ViewModels.Mappings;

public class MappingDetailViewModel
{
    public Guid MappingBrokerId { get; set; }

    public dynamic Source { get; set; } = new object();

    public dynamic Destination { get; set; } = new object();

    public List<PropertyInfo> Properties => Destination.GetType().GetProperties();
}