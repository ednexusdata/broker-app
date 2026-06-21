using Ardalis.Specification;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications;

public class JobsByTypeAndStatus : Specification<Job>, ISingleResultSpecification
{
    public JobsByTypeAndStatus(string jobType)
    {
        Query
            .Where(j => j.JobType == jobType && (j.JobStatus == JobStatus.Waiting || j.JobStatus == JobStatus.Running));
    }
}
