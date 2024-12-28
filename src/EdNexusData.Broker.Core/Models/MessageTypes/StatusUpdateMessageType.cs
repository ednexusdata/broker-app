using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.MessageTypes;

public class StatusUpdateMessageType
{
    public RequestStatus StatusUpdate { get; set; } = default!;
}