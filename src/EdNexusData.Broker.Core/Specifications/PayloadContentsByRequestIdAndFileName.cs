using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class PayloadContentsByRequestIdAndFileName : Specification<PayloadContent>, ISingleResultSpecification
{
  public PayloadContentsByRequestIdAndFileName(Guid requestId, string fileName)
  {
    Query
        .Where(req => req.RequestId == requestId && req.FileName == fileName);
  }
}