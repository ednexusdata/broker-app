using System.ComponentModel;

namespace EdNexusData.Broker.Core;

public enum MatchDisposition
{
    [Description("Found")]
    Found,
    
    [Description("Not Found")]
    NotFound
}