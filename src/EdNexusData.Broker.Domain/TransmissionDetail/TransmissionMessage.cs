namespace EdNexusData.Broker.Domain;

public class TransmissionMessage
{
    public TransmissionContent? Request { get; set; }
    public TransmissionContent? Response { get; set; }
}