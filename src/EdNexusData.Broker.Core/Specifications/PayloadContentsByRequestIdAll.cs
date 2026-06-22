using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class PayloadContentsByRequestIdAll : Specification<PayloadContent>
{
    public PayloadContentsByRequestIdAll(Guid requestId)
    {
        Query
            .Where(pc => pc.RequestId == requestId);
    }
}
