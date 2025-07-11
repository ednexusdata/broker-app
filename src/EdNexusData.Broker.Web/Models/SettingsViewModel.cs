namespace EdNexusData.Broker.Web.Models;

public class SettingsViewModel
{
    public List<Type>? ConnectorTypes { get; set; }

    public List<Type>? PayloadTypes { get; set; }

    public List<EducationOrganizationConnectorSettings>? ConnectorSettings { get; set; }

    public List<dynamic>? Models { get; set; } = new List<dynamic>();
}