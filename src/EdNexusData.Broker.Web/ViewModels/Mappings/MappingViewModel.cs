using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Domain;
using System.Reflection;
using EdNexusData.Broker.Connector;
using EdNexusData.Broker.Service.Lookup;

namespace EdNexusData.Broker.Web.ViewModels.Mappings;

public class MappingViewModel
{
    public MappingLookupService? MappingLookupService { get; set; }
    
    public Guid? MappingId { get; set; }

    public List<Mapping>? RequestMappings { get; set; }

    public Mapping? Mapping { get; set; }

    public dynamic? MappingSourceRecords { get; set; }  
    public dynamic? MappingDestinationRecords { get; set; }

    public PropertyInfo[]? Properties { get; set; }

    public List<PropertyInfo>? EditingProperties { get; set; } = new List<PropertyInfo>();

    public DisplayNameAttribute? ResolveMappingTypeDisplayName(string mappingTypeName)
    {
        var mappingType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mappingTypeName).FirstOrDefault();
        return (DisplayNameAttribute)mappingType?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!;
    }

    public DisplayNameAttribute? GetPropertyDisplayName(PropertyInfo property)
    {
        return (DisplayNameAttribute)property?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayNameAttribute)).FirstOrDefault()!;
    }

    public DataTypeAttribute? GetPropertyDataType(PropertyInfo property)
    {
        var dataType = property?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DataTypeAttribute));
        if (dataType is not null && dataType.Count() > 0)
        {
            return (DataTypeAttribute)dataType.FirstOrDefault()!;
        }
        return null;
    }

    public static LookupAttribute? GetPropertyLookupType(PropertyInfo property)
    {
        var dataType = property?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(LookupAttribute));
        if (dataType is not null && dataType.Count() > 0)
        {
            return (LookupAttribute)dataType.FirstOrDefault()!;
        }
        return null;
    }

    public void SetProperties(string mappingType)
    {
        Properties = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mappingType).FirstOrDefault()!
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

        // Loop through each property and see if datatype on property
        foreach(var property in Properties)
        {
            var dataPropertyType = GetPropertyDataType(property);
            if (dataPropertyType is not null)
            {
                EditingProperties!.Add(property);
            }
        }
    }

    // public static object BrokerIdForObject(dynamic obj)
    // {
        
    // }

    public static object ValueForProperty(dynamic obj, PropertyInfo property)
    {
        return property.GetValue(obj);
    }
    
    public static object? ValueForProperty(dynamic mappingDestinationObj, dynamic mappingSourceObj, PropertyInfo property)
    {
        var brokerId = mappingDestinationObj.BrokerId;
        
        // Lookup the matching source record
        foreach(var maptest in mappingSourceObj)
        {
           if (brokerId == maptest.BrokerId)
           {
                return property.GetValue(maptest);
           }
        }
        
        return null;
    }

    public static object InputName(string mappingType, int counter, PropertyInfo property)
    {
        return InputName(mappingType, counter, property.Name);
    }

    public static object InputName(string mappingType, int counter, string propertyName)
    {
        var mappingTyped = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mappingType).FirstOrDefault()!;
        
        return $"mapping[{counter}].{propertyName}"; // .{mappingTyped!.Name}
    }
}