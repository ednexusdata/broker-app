using Ardalis.Specification;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications;

// Slim companion to PayloadContentActionWithPayloadContent — only pulls in what's needed to resolve
// the owning Request's id (e.g. for activity-log RequestId resolution), not the full ed-org chain.
public class PayloadContentActionWithPayloadContentId : Specification<PayloadContentAction>, ISingleResultSpecification
{
    public PayloadContentActionWithPayloadContentId(Guid actionId)
    {
        Query
            .Include(x => x.PayloadContent)
            .Where(x => x.Id == actionId);
    }
}
