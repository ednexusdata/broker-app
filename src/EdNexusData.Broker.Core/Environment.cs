using System.Collections.Immutable;

namespace EdNexusData.Broker.Core;

public abstract class Environment
{
    public string EnvironmentName = default!;
    public List<Uri> Addresses = new List<Uri>();

    public static ImmutableList<string> NonProductionToLocalEnvironments => new List<string> { "demo", "development", "dev" }.ToImmutableList();
    public static ImmutableList<string> NonProductionEnvironments => new List<string> { "train", "training", "test", "testing" }.ToImmutableList();
    public static ImmutableList<string> ProductionEnvironments => new List<string> { "production", "live", "prod" }.ToImmutableList();

    public bool IsNonProductionEnvironment()
    {
        return NonProductionEnvironments.Contains(EnvironmentName.ToLower());
    }

    public bool IsNonProductionToLocalEnvironment()
    {
        return NonProductionToLocalEnvironments.Contains(EnvironmentName.ToLower());
    }

    public bool IsProductionEnvironment()
    {
        return ProductionEnvironments.Contains(EnvironmentName.ToLower());
    }

    public void AddAddress(string uri)
    {
        Addresses.Add(new Uri(uri));
    }
}