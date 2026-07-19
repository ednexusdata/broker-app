using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

// Slim companion to MappingWithPayloadContent — only pulls in what's needed to resolve the owning
// Request's id (e.g. for activity-log RequestId resolution), not the full ed-org chain.
public class MappingWithPayloadContentId : Specification<Mapping>, ISingleResultSpecification
{
    public MappingWithPayloadContentId(Guid id)
    {
        Query
            .Include(x => x.PayloadContentAction)
            .ThenInclude(x => x!.PayloadContent)
            .Where(x => x.Id == id);
    }
}
