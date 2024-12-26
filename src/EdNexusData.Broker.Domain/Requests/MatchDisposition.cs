using System.ComponentModel;

namespace EdNexusData.Broker.Domain;

public enum MatchDisposition
{
    [Description("Found")]
    Found,
    
    [Description("Not Found")]
    NotFound
}