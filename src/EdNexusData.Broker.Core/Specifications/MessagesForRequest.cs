using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class MessagesForRequest : Specification<Message>, ISingleResultSpecification
{
  
  public MessagesForRequest(Guid requestId, Type? messageType = null)
  {
    if (messageType is not null)
    {
      Query.Where(x => x.MessageType == messageType.FullName);
    }
    
    Query
        .Where(x => x.RequestId == requestId)
        .OrderBy(x => x.CreatedAt);
  }
}