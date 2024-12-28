using Ardalis.Specification;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications;

public class PayloadContentActionWithPayloadContent : Specification<PayloadContentAction>, ISingleResultSpecification
{
  public PayloadContentActionWithPayloadContent(Guid actionid)
  {
    Query
        .Include(x => x.ActiveMapping)
        .Include(x => x.PayloadContent)
        .ThenInclude(x => x!.Request)
        .ThenInclude(x => x!.EducationOrganization)
        .ThenInclude(x => x!.ParentOrganization)
        .Where(x => x.Id == actionid);
  }
}