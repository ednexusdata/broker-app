namespace EdNexusData.Broker.Core.Emails.ViewModels;

public class RetentionReminderViewModel : BaseViewModel
{
    public Student? Student { get; set; }
    public Guid RequestId { get; set; }
    public int DaysRemaining { get; set; }
    public DateTimeOffset DestructionAt { get; set; }
}
