using Ardalis.Specification;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications;

public class JobsWaitingToProcess : Specification<Job>, ISingleResultSpecification
{
  public JobsWaitingToProcess()
  {
    var requestStatuses = new JobStatus[] { JobStatus.Waiting };

    Query
        .Where(req => requestStatuses.Contains(req.JobStatus));
  }
}