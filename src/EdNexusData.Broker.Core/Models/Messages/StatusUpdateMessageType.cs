using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Messages;

public class StatusUpdateMessageType
{
    public RequestStatus StatusUpdate { get; set; } = default!;
}