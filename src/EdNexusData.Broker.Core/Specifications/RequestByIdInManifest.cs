using Ardalis.Specification;

namespace EdNexusData.Broker.Core.Specifications;

public class RequestByIdInManifest : Specification<Request>, ISingleResultSpecification
{
  public RequestByIdInManifest(Guid requestId, Guid educationOrganizationId)
  {
    Query
        .Where(req => 
          (req.RequestManifest!.RequestId == requestId || req.ResponseManifest!.RequestId == requestId)
          && req.EducationOrganizationId != educationOrganizationId
        );
  }
}