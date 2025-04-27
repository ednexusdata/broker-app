namespace EdNexusData.Broker.Core.Models;

public class EmailJobDetail
{
    public string? TemplateName { get; set; }

    public string? ModelType { get; set; }

    public object? Model { get; set; }

    public string? To { get; set; }
    public string? ReplyTo { get; set; }

    public string? Subject { get; set; }
}