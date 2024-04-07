namespace EdNexusData.Broker.Web.ViewModels.Settings;

public class PayloadSettingsViewModel
{
    public string FullName { get; set; } = default!;

    public string DisplayName { get; set; } = default!;

    public string? Configuration { get; set; }
}