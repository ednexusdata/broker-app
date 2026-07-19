namespace EdNexusData.Broker.Core.Emails.ViewModels;

public class RetentionDestroyedViewModel : BaseViewModel
{
    public Student? Student { get; set; }
    public Guid RequestId { get; set; }
    public RequestAddress? From { get; set; }
    public RequestAddress? To { get; set; }
    public DateTimeOffset LastActivityAt { get; set; }
    public DateTimeOffset DestroyedAt { get; set; }
    public int RetentionDays { get; set; }
}
