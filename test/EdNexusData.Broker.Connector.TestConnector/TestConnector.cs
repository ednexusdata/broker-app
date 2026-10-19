using System.ComponentModel;
using EdNexusData.Broker.Common.Configuration;
using EdNexusData.Broker.Common.Connector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EdNexusData.Broker.Connector.TestConnector;

[DisplayName("Test Connector")]
public class Connector : IConnector
{
    public static string DisplayName { get; } = "Test Connector";
    public static string AuthorName { get; } = "Education Nexus";
    public static string AbbreviatedAuthordName { get; } = "EdNexusData";
}

public class ServiceCollection : IConnectorServiceCollection
{
    public static IServiceCollection AddDependencies(IServiceCollection services)
    {
        services.AddScoped<ScopedService>();
        services.AddSingleton<SingletonService>();
        services.AddScoped(typeof(GenericService<>));
        return services;
    }
}

/// <summary>Scoped connector service that depends on an app (scoped) service.</summary>
public class ScopedService
{
    public ScopedService(IConfigurationResolver configurationResolver)
    {
        ConfigurationResolver = configurationResolver;
    }

    public IConfigurationResolver ConfigurationResolver { get; }
}

/// <summary>Singleton connector service taking an app open generic closed over a connector type.</summary>
public class SingletonService
{
    public SingletonService(ILogger<SingletonService> logger)
    {
        Logger = logger;
    }

    public ILogger<SingletonService> Logger { get; }
}

/// <summary>Open-generic connector service.</summary>
public class GenericService<T> where T : class
{
}

public class Widget
{
    public string? Name { get; set; }
}

/// <summary>Not registered: created through ConnectorActivator, like connector jobs and transformers.</summary>
public class ActivatedJob
{
    public ActivatedJob(ScopedService scoped, SingletonService singleton, GenericService<Widget> generic, ILogger<ActivatedJob> logger)
    {
        Scoped = scoped;
        Singleton = singleton;
        Generic = generic;
        Logger = logger;
    }

    public ScopedService Scoped { get; }
    public SingletonService Singleton { get; }
    public GenericService<Widget> Generic { get; }
    public ILogger<ActivatedJob> Logger { get; }
}
