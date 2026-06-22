using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class PayloadContentActionsByPayloadContentId : Specification<PayloadContentAction>
{
    public PayloadContentActionsByPayloadContentId(Guid payloadContentId)
    {
        Query
            .Where(a => a.PayloadContentId == payloadContentId);
    }
}
