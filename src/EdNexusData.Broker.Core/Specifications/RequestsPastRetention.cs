using Ardalis.Specification;
using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Specifications;

public class RequestsPastRetention : Specification<Request>
{
    public RequestsPastRetention(DateTimeOffset cutoffDate)
    {
        Query
            .Where(r => (r.RequestStatus == RequestStatus.Finished || r.RequestStatus == RequestStatus.Closed) && r.UpdatedAt <= cutoffDate);
    }
}
