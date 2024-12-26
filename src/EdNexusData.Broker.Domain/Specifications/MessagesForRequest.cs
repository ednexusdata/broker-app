using Ardalis.Specification;

namespace EdNexusData.Broker.Domain.Specifications;

public class MessagesForRequest : Specification<Message>, ISingleResultSpecification
{
  
  public MessagesForRequest(Guid requestId)
  {
    Query
        .Where(x => x.RequestId == requestId)
        .OrderBy(x => x.CreatedAt);
  }
}