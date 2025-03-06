using System.Text.Json;
using EdNexusData.Broker.Common.Mappings;

namespace EdNexusData.Broker.Core;

public class Mapping : BaseEntity, IAggregateRoot
{
    public Guid? PayloadContentActionId { get; set; }
    public PayloadContentAction? PayloadContentAction { get; set; }
    public PayloadContentAction? PrimaryPayloadContentAction { get; set; }
    public PayloadContentSchema? OriginalSchema { get; set; }
    public string? MappingType { get; set; }
    public StudentAttributes? StudentAttributes { get; set; }
    public JsonDocument? JsonSourceMapping { get; set; }
    public JsonDocument? JsonInitialMapping { get; set; }
    public JsonDocument? JsonDestinationMapping { get; set; }
    public byte Version { get; set; } = 1;

    public int? ReceviedCount { 
        get {
            return JsonSourceMapping?.RootElement.EnumerateArray().Count();
        } 
    }
    public int? IgnoredCount {  
        get {
            return JsonDestinationMapping?.RootElement.EnumerateArray().Where(x => x.GetProperty("BrokerMappingRecordAction").GetUInt16() == (int)MappingRecordAction.Ignore).Count();
        }
    }
    public int? MappedCount {
        get {
            return JsonDestinationMapping?.RootElement.EnumerateArray().Where(x => x.GetProperty("BrokerMappingRecordAction").GetUInt16() == (int)MappingRecordAction.Import).Count();
        }
    }
    public int? RemainingCount { get { return ReceviedCount - IgnoredCount - MappedCount; } }

    public Common.Mappings.Mapping ToCommon()
    {
        return new Common.Mappings.Mapping()
        {
            Id = Id,
            PayloadContentActionId = PayloadContentActionId,
            // PayloadContentAction = PayloadContentAction?.ToCommon(),
            // PrimaryPayloadContentAction = PayloadContentAction?.ToCommon(),
            OriginalSchema = OriginalSchema?.ToCommon(),
            MappingType = MappingType,
            StudentAttributes = StudentAttributes?.ToCommon(),
            JsonSourceMapping = JsonSourceMapping,
            JsonInitialMapping = JsonInitialMapping,
            JsonDestinationMapping = JsonDestinationMapping,
            Version = Version
        };
    }
}
