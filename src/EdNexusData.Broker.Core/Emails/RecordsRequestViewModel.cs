namespace EdNexusData.Broker.Core.Emails.ViewModels;

public class RecordsRequestViewModel : BaseViewModel
{
    public Student? Student { get; set; }
    public RequestAddress? From { get; set; }
    public string? Note { get; set; }
    public Guid? RequestId { get; set; }
}