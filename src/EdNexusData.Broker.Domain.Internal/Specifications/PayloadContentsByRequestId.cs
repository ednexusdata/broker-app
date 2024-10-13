using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Internal.Specifications;

public class PayloadContentsByRequestId : Specification<PayloadContent>, ISingleResultSpecification
{
  public PayloadContentsByRequestId(Guid requestId)
  {
    Query
        .Where(req => req.RequestId == requestId && req.MessageId == null);
  }
}