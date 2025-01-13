using EdNexusData.Broker.Common.Jobs;

namespace EdNexusData.Broker.Core.Messages;

public class StatusUpdateMessage
{
    public RequestStatus StatusUpdate { get; set; } = default!;
}