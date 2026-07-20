namespace EdNexusData.Broker.Core.Specifications.Tests.Unit;

public class MappingWithPayloadContentIdTests
{
    [Fact]
    public void MappingWithPayloadContentId_ReturnsMatchingMapping()
    {
        var mappingId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var matching = new Mapping
        {
            Id = mappingId,
            PayloadContentAction = new PayloadContentAction
            {
                Id = Guid.NewGuid(),
                PayloadContentId = Guid.NewGuid(),
                PayloadContent = new PayloadContent { RequestId = requestId }
            }
        };
        var other = new Mapping { Id = Guid.NewGuid() };

        var result = new MappingWithPayloadContentId(mappingId).Evaluate(new List<Mapping> { matching, other });

        var found = Assert.Single(result);
        Assert.Equal(requestId, found.PayloadContentAction?.PayloadContent?.RequestId);
    }
}
