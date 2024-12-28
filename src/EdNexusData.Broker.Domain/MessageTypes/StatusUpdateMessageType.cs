using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Domain.MessageTypes;

public class StatusUpdateMessageType
{
    public RequestStatus StatusUpdate { get; set; } = default!;
}