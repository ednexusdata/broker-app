using System.Collections.Immutable;

namespace EdNexusData.Broker.Core;

public abstract class Environment
{
    public string EnvironmentName = default!;
    public List<Uri> Addresses = new List<Uri>();

    public static ImmutableList<string> NonProductionEnvironments => new List<string> { "Demo", "Development", "Test" }.ToImmutableList();
    public static ImmutableList<string> ProductionEnvironments => new List<string> { "Production", "Live" }.ToImmutableList();

    public bool IsNonProductionEnvironment()
    {
        return !ProductionEnvironments.Contains(EnvironmentName);
    }

    public bool IsProductionEnvironment()
    {
        return ProductionEnvironments.Contains(EnvironmentName);
    }

    public void AddAddress(string uri)
    {
        Addresses.Add(new Uri(uri));
    }
}