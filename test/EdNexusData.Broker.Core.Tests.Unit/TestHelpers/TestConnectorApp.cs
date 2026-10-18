using Autofac;
using Autofac.Extensions.DependencyInjection;
using EdNexusData.Broker.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Core.Tests.Unit.TestHelpers;

/// <summary>
/// An app container wired like the Web and Worker hosts (Autofac, ConnectorLoader, ConnectorActivator), over a
/// temporary connectors folder holding copies of the test connector built from EdNexusData.Broker.Connector.TestConnector.
/// </summary>
public sealed class TestConnectorApp : IDisposable
{
    public const string ConnectorName = "EdNexusData.Broker.Connector.TestConnector";
    public const string Namespace = "EdNexusData.Broker.Connector.TestConnector";

    private static readonly string BuiltConnector = Path.Combine(AppContext.BaseDirectory, "TestConnector");

    private readonly IContainer _container;

    public string ConnectorsPath { get; }
    public ConnectorLoader Loader { get; }
    public IServiceProvider Services { get; }
    public IContainer Container => _container;

    private TestConnectorApp(string connectorsPath)
    {
        ConnectorsPath = connectorsPath;

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddLogging();
        services.AddScoped<IConfigurationResolver, FakeConfigurationResolver>();

        // Same registration the hosts get from AddConnectorServices, but over the temporary folder.
        Loader = new ConnectorLoader(connectorsPath);
        Loader.UseAppServices(services);
        services.AddSingleton(Loader);
        services.AddScoped<ConnectorActivator>();

        var builder = new ContainerBuilder();
        builder.Populate(services);
        _container = builder.Build();
        Services = new AutofacServiceProvider(_container);
    }

    /// <summary>Creates an app with the test connector installed unversioned (connectors/&lt;name&gt;/*.dll).</summary>
    public static TestConnectorApp Create()
    {
        var root = CreateConnectorsFolder();
        CopyConnectorTo(Path.Combine(root, ConnectorName));
        return new TestConnectorApp(root);
    }

    /// <summary>Creates an app over a connectors folder prepared by <paramref name="prepare"/>.</summary>
    public static TestConnectorApp Create(Action<string> prepare)
    {
        var root = CreateConnectorsFolder();
        prepare(root);
        return new TestConnectorApp(root);
    }

    public static string CreateConnectorsFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), "broker-connector-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    /// <summary>Copies the built test connector into <paramref name="directory"/>.</summary>
    public static string CopyConnectorTo(string directory)
    {
        Directory.CreateDirectory(directory);
        foreach (var file in Directory.GetFiles(BuiltConnector))
        {
            File.Copy(file, Path.Combine(directory, Path.GetFileName(file)), overwrite: true);
        }
        return directory;
    }

    /// <summary>A copy of the test connector to load as a "new version" of it.</summary>
    public string CreateNewVersionFolder(string version = "2.0.0") =>
        CopyConnectorTo(Path.Combine(ConnectorsPath, ConnectorName, version));

    public ConnectorRuntime Runtime => Loader.Runtimes.Single(r => r.Name == ConnectorName);

    public Type ConnectorType(string name) => Runtime.Assemblies.Single().GetType($"{Namespace}.{name}", throwOnError: true)!;

    public void Dispose()
    {
        _container.Dispose();
        try
        {
            Directory.Delete(ConnectorsPath, recursive: true);
        }
        catch (IOException)
        {
            // Files can stay locked on Windows if a load context could not be unloaded.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}

public class FakeConfigurationResolver : IConfigurationResolver
{
    public Guid? CurrentRecordEducationOrganizationId { get; set; }

    public Task<T> FetchConnectorSettingsAsync<T>() => throw new NotSupportedException();

    public Task<T> FetchConnectorSettingsAsync<T>(string districtNumber) => throw new NotSupportedException();
}
