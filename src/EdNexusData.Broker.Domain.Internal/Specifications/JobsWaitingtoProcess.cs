using Ardalis.Specification;
using EdNexusData.Broker.Domain.Worker;

namespace EdNexusData.Broker.Domain.Internal.Specifications;

public class JobsWaitingToProcess : Specification<Job>, ISingleResultSpecification
{
  public JobsWaitingToProcess()
  {
    var requestStatuses = new JobStatus[] { JobStatus.Waiting };

    Query
        .Where(req => requestStatuses.Contains(req.JobStatus));
  }
}