using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class MappingsByPayloadContentActionId : Specification<Mapping>
{
    public MappingsByPayloadContentActionId(Guid actionId)
    {
        Query
            .Where(m => m.PayloadContentActionId == actionId);
    }
}
