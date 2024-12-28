using Ardalis.Specification;
using EdNexusData.Broker.Core.Worker;

namespace EdNexusData.Broker.Core.Specifications;

public class ActionByPayloadContentActionType : Specification<PayloadContentAction>, ISingleResultSpecification
{
  public ActionByPayloadContentActionType(Guid payloadContentId, string actionType)
  {
    Query
        .Where(x => x.PayloadContentId == payloadContentId 
            && x.PayloadContentActionType == actionType);
  }
}