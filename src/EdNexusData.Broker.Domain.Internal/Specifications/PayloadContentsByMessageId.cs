using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Internal.Specifications;

public class PayloadContentsByMessageId : Specification<PayloadContent>, ISingleResultSpecification
{
  public PayloadContentsByMessageId(Guid messageId)
  {
    Query
        .Where(req => req.MessageId == messageId);
  }
}