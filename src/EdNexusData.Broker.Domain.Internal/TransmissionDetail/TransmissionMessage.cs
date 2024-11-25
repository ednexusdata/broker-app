namespace EdNexusData.Broker.Domain.Internal;

public class TransmissionMessage
{
    public TransmissionContent? Request { get; set; }
    public TransmissionContent? Response { get; set; }
}