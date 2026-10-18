namespace EdNexusData.Broker.Core.Specifications.Tests.Unit;

public class PayloadContentActionWithPayloadContentIdTests
{
    [Fact]
    public void PayloadContentActionWithPayloadContentId_ReturnsMatchingAction()
    {
        var actionId = Guid.NewGuid();
        var requestId = Guid.NewGuid();

        var matching = new PayloadContentAction
        {
            Id = actionId,
            PayloadContentId = Guid.NewGuid(),
            PayloadContent = new PayloadContent { RequestId = requestId }
        };
        var other = new PayloadContentAction { Id = Guid.NewGuid(), PayloadContentId = Guid.NewGuid() };

        var result = new PayloadContentActionWithPayloadContentId(actionId).Evaluate(new List<PayloadContentAction> { matching, other });

        var found = Assert.Single(result);
        Assert.Equal(requestId, found.PayloadContent?.RequestId);
    }
}
