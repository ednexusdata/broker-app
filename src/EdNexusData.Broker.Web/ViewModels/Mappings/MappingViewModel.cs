using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using EdNexusData.Broker.Common.Lookup;
using EdNexusData.Broker.Common.Mappings;
using EdNexusData.Broker.Core.Lookup;

namespace EdNexusData.Broker.Web.ViewModels.Mappings;

public class MappingViewModel
{
    public MappingLookupService? MappingLookupService { get; set; }
    
    public Guid? MappingId { get; set; }

    public Guid? RequestId { get; set; }

    public Request? Request { get; set; }

    public bool IsValid = true;

    public List<Core.Mapping>? RequestMappings { get; set; }

    public Core.Mapping? Mapping { get; set; }

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

    public bool PropertyRequired(PropertyInfo property)
    {
        var required = property?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(RequiredAttribute));
        if (required is not null && required.Count() > 0)
        {
            return true;
        }
        return false;
    }

    public DataTypeAttribute? GetPropertyDataType(PropertyInfo property)
    {
        if (property?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DisplayInTableAttribute)).Count() > 0)
        {

        
        var dataType = property?.GetCustomAttributes(false).Where(x => x.GetType() == typeof(DataTypeAttribute));
        if (dataType is not null && dataType.Count() > 0)
        {
            return (DataTypeAttribute)dataType.FirstOrDefault()!;
        }

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
    
    public static object InputId(string mappingType, int counter, PropertyInfo property)
    {
        return InputId(mappingType, counter, property.Name);
    }

    public static object InputName(string mappingType, int counter, string propertyName)
    {
        var mappingTyped = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mappingType).FirstOrDefault()!;

        return $"mapping[{counter}].{propertyName}"; // .{mappingTyped!.Name}
    }
    
    public static object InputId(string mappingType, int counter, string propertyName)
    {
        var mappingTyped = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.FullName == mappingType).FirstOrDefault()!;
        
        return $"mapping_{counter}_{propertyName}"; // .{mappingTyped!.Name}
    }
}