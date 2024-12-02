namespace EdNexusData.Broker.Domain.Internal.MessageTypes;

public class StatusUpdateMessageType
{
    public RequestStatus StatusUpdate { get; set; } = default!;
}