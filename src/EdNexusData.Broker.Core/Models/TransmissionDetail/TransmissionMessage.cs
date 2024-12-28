namespace EdNexusData.Broker.Core;

public class TransmissionMessage
{
    public TransmissionContent? Request { get; set; }
    public TransmissionContent? Response { get; set; }
}