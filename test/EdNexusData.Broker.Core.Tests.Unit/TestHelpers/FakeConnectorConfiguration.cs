using EdNexusData.Broker.Common.Configuration;

namespace EdNexusData.Broker.Core.Tests.Unit.TestHelpers;

/// <summary>
/// Stand-in for a real connector's settings class (e.g. EdupointSynergyConfiguration) used to
/// exercise ConfigurationSerializer/ConfigurationResolver without depending on an actual connector.
/// </summary>
public class FakeConnectorConfiguration : IConfiguration
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
