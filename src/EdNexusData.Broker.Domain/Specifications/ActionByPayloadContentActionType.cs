using Ardalis.Specification;
using EdNexusData.Broker.Domain.Worker;

namespace EdNexusData.Broker.Domain.Specifications;

public class ActionByPayloadContentActionType : Specification<PayloadContentAction>, ISingleResultSpecification
{
  public ActionByPayloadContentActionType(Guid payloadContentId, string actionType)
  {
    Query
        .Where(x => x.PayloadContentId == payloadContentId 
            && x.PayloadContentActionType == actionType);
  }
}