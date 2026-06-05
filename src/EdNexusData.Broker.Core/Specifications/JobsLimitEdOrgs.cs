using Ardalis.Specification;
using EdNexusData.Broker.Common.Jobs;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications;

public class JobsLimitEdOrgs : Specification<Job>, ISingleResultSpecification
{
  public JobsLimitEdOrgs(Guid? id, List<EducationOrganization> limitedEdOrgs)
  {
    Query
        .Include(x => x.EducationOrganization)
        .Where(x => limitedEdOrgs.Contains(x.EducationOrganization!));
  }
}