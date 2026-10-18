using Ardalis.Specification;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications;

public class JobsByReferenceAndType : Specification<Job>
{
    public JobsByReferenceAndType(string jobType, Guid referenceGuid)
    {
        Query
            .AsNoTracking()
            .Where(j => j.JobType == jobType && j.ReferenceGuid == referenceGuid);
    }
}
