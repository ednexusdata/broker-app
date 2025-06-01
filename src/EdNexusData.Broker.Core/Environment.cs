using System.Collections.Immutable;
using EdNexusData.Broker.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Core;

public abstract class Environment
{
    public string EnvironmentName = default!;
    public ApplicationName ApplicationName = default!;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly SettingsService settingsService;
    public Uri BrokerBaseUrl
    {
        get
        {
            if (settingsService == null)
            {
                throw new InvalidOperationException("SettingsService is not initialized.");
            }

            var brokerBaseUrl = settingsService.GetValueAsync("BrokerBaseUrl").GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(brokerBaseUrl))
            {
                return new Uri("https://[::]");
            }

            try
            {
                return new Uri(brokerBaseUrl);
            }
            catch (UriFormatException)
            {
                throw new ArgumentException("Invalid BrokerBaseUrl format.");
            }
        }
    }

    public Uri LocalBrokerUrl
    {
        get
        {
            if (settingsService == null)
            {
                throw new InvalidOperationException("SettingsService is not initialized.");
            }

            var workerUrl = settingsService.GetValueAsync("LocalBrokerUrl").GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(workerUrl))
            {
                return BrokerBaseUrl;
            }

            try
            {
                return new Uri(workerUrl);
            }
            catch (UriFormatException)
            {
                throw new ArgumentException("Invalid LocalBrokerUrl format.");
            }
        }
    }

    public string? BrokerBaseUrlWithoutSlash => BrokerBaseUrl?.ToString().TrimEnd('/');

    public static ImmutableList<string> NonProductionToLocalEnvironments => new List<string> { "demo", "development", "dev" }.ToImmutableList();
    public static ImmutableList<string> NonProductionEnvironments => new List<string> { "train", "training", "test", "testing" }.ToImmutableList();
    public static ImmutableList<string> ProductionEnvironments => new List<string> { "production", "live", "prod" }.ToImmutableList();

    public Environment(
        IServiceScopeFactory serviceScopeFactory
    )
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.settingsService = this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<SettingsService>();
    }

    public bool IsNonProductionEnvironment()
    {
        return NonProductionEnvironments.Contains(EnvironmentName.ToLower());
    }

    public bool IsNonProductionToLocalEnvironment()
    {
        return NonProductionToLocalEnvironments.Contains(EnvironmentName.ToLower());
    }

    public static bool IsNonProductionToLocalEnvironment(string environmentName)
    {
        return NonProductionToLocalEnvironments.Contains(environmentName.ToLower());
    }

    public bool IsProductionEnvironment()
    {
        return ProductionEnvironments.Contains(EnvironmentName.ToLower());
    }

}

public enum ApplicationName
{
    EdNexusDataBrokerWeb,
    EdNexusDataBrokerWorker
}