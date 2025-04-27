using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Common.Mappings;

namespace EdNexusData.Broker.Core.Services;

public class MappingRecordValidatorService
{
    public MappingRecordValidatorService
    (

    )
    {
        
    }

    public bool ValidateRecord(dynamic mappingRecord)
    {
        _ = mappingRecord ?? throw new ArgumentNullException(nameof(mappingRecord), "Mapping record cannot be null.");
        
        // Get properties
        Type recordType = mappingRecord.GetType();
        var properties = recordType.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
        
        // Loop through properties
        foreach(var property in properties.Where(x => x.Name != "BrokerId" && x.Name != "IsValid"))
        {
            // Check if required
            var required = property.GetCustomAttributes(false).Where(x => x.GetType() == typeof(RequiredAttribute)).Count() > 0;

            if (required)
            {
                // Check if null
                var value = property.GetValue(mappingRecord);
                if (value == null)
                {
                    return false;
                }
                
                // Check if empty string
                if (value is string strValue && string.IsNullOrWhiteSpace(strValue))
                {
                    return false;
                }
                
                // Check if empty array
                if (value is Array arrValue && arrValue.Length == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public IEnumerable<dynamic> ValidateRecords(IEnumerable<dynamic> mappingRecord)
    {
        _ = mappingRecord ?? throw new ArgumentNullException(nameof(mappingRecord), "Mapping record cannot be null.");
        
        // Loop through records
        foreach (var record in mappingRecord)
        {
            Type recordType = record.GetType();
            var actionProperty = recordType.GetProperties().Where(prop => prop.CanRead && prop.CanWrite && prop.Name == "BrokerMappingRecordAction").FirstOrDefault();
            _ = actionProperty ?? throw new ArgumentNullException(nameof(actionProperty), "Action property not found on record.");
            
            var isValidProperty = recordType.GetProperties().Where(prop => prop.CanRead && prop.CanWrite && prop.Name == "IsValid").FirstOrDefault();
            _ = isValidProperty ?? throw new ArgumentNullException(nameof(isValidProperty), "IsValid property not found on record.");

            // Get Action property
            var actionString = actionProperty.GetValue(record);
            var action = (MappingRecordAction)Enum.Parse(typeof(MappingRecordAction), actionString.ToString());

            if (action == MappingRecordAction.Import && !ValidateRecord(record))
            {
                isValidProperty?.SetValue(record, false);
            }
            else
            {
                isValidProperty?.SetValue(record, true);
            }
        }

        return mappingRecord;
    }

    public bool IsValidRecords(IEnumerable<dynamic> mappingRecord)
    {
        _ = mappingRecord ?? throw new ArgumentNullException(nameof(mappingRecord), "Mapping record cannot be null.");
        
        // Loop through records
        foreach (var record in mappingRecord)
        {
            Type recordType = record.GetType();
            var isValidProperty = recordType.GetProperties().Where(prop => prop.CanRead && prop.CanWrite && prop.Name == "IsValid").FirstOrDefault();
            _ = isValidProperty ?? throw new ArgumentNullException(nameof(isValidProperty), "IsValid property not found on record.");

            var isValid = (bool)isValidProperty.GetValue(record);
            if (!isValid)
            {
                return false;
            }
        }

        return true;
    }
}