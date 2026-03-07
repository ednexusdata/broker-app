using Ardalis.Specification;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications;

public class JobsWithEdOrgById : Specification<Job>, ISingleResultSpecification
{
  public JobsWithEdOrgById(Guid? id)
  {
    Query
        .Include(x => x.EducationOrganization)
        .Where(x => x.Id == id);
  }
}