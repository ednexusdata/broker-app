using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Autofac;
using EdNexusData.Broker.Core.Tests.Unit.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using File = System.IO.File;

namespace EdNexusData.Broker.Core.Tests.Unit.Connector;

[CollectionDefinition(Name, DisableParallelization = true)]
public class ConnectorCollection
{
    // Unload tests force garbage collections and check that load contexts are released, so they run on their own.
    public const string Name = "Connectors";
}

[Collection(ConnectorCollection.Name)]
public class ConnectorLoaderTests
{
    private const string Name = TestConnectorApp.ConnectorName;

    [Fact]
    public void Loads_connector_into_its_own_collectible_load_context()
    {
        using var app = TestConnectorApp.Create();

        var runtime = Assert.Single(app.Loader.Runtimes);
        Assert.Equal(Name, runtime.Name);
        Assert.True(runtime.LoadContext.IsCollectible);
        Assert.Same(runtime.LoadContext, AssemblyLoadContext.GetLoadContext(runtime.ConnectorType.Assembly));
        Assert.Equal(new Version(1, 0, 0, 0), runtime.Version);
    }

    [Fact]
    public void Indexes_connector_in_the_existing_loader_views()
    {
        using var app = TestConnectorApp.Create();

        var connectorType = app.ConnectorType("Connector");
        Assert.Contains(connectorType, app.Loader.Connectors);
        Assert.Same(connectorType, app.Loader.GetConnector(connectorType.FullName!));
        Assert.Same(connectorType.Assembly, app.Loader.Assemblies[Name]);
        Assert.Same(app.Runtime.LoadContext, app.Loader.ConnectorLoadContexts[$"ConnectorContext_{Name}"]);
    }

    [Fact]
    public void Collects_the_services_the_connector_registers_without_adding_them_to_the_app()
    {
        using var app = TestConnectorApp.Create();

        var services = app.Runtime.Services.ToDictionary(s => s.ServiceType.Name, s => s.Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, services["ScopedService"]);
        Assert.Equal(ServiceLifetime.Singleton, services["SingletonService"]);
        Assert.Equal(ServiceLifetime.Scoped, services["GenericService`1"]);

        Assert.False(app.Container.IsRegistered(app.ConnectorType("ScopedService")));
    }

    [Fact]
    public void Shared_contracts_come_from_the_app_not_the_connector_folder()
    {
        using var app = TestConnectorApp.Create();

        Assert.DoesNotContain(app.Runtime.Assemblies, a => a.GetName().Name == "EdNexusData.Broker.Common");
        Assert.Contains(typeof(EdNexusData.Broker.Common.Connector.IConnector), app.ConnectorType("Connector").GetInterfaces());
    }

    [Fact]
    public void Loads_the_highest_version_subfolder()
    {
        string? expected = null;
        using var app = TestConnectorApp.Create(root =>
        {
            TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name, "2.0.0"));
            expected = TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name, "10.0.0"));
            TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name, "9.1.0"));
        });

        Assert.Equal(expected, app.Runtime.Directory);
        Assert.Equal(Name, app.Runtime.Name);
    }

    [Fact]
    public void Starts_with_the_newest_version_that_loads_when_a_newer_one_is_broken()
    {
        string? working = null;
        using var app = TestConnectorApp.Create(root =>
        {
            working = TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name, "1.0.0"));
            var broken = Directory.CreateDirectory(Path.Combine(root, Name, "2.0.0")).FullName;
            File.WriteAllText(Path.Combine(broken, $"{Name}.dll"), "not a .NET assembly");
        });

        Assert.Equal(working, app.Runtime.Directory);
    }

    [Fact]
    public void Starts_without_a_connector_none_of_whose_versions_load()
    {
        using var app = TestConnectorApp.Create(root =>
        {
            TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name));
            var broken = Directory.CreateDirectory(Path.Combine(root, "Broken.Connector", "1.0.0")).FullName;
            File.WriteAllText(Path.Combine(broken, "Broken.Connector.dll"), "not a .NET assembly");
        });

        Assert.Equal([Name], app.Loader.Runtimes.Select(r => r.Name));
    }

    [Fact]
    public void Skips_connector_folders_without_assemblies()
    {
        using var app = TestConnectorApp.Create(root =>
        {
            TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name));
            Directory.CreateDirectory(Path.Combine(root, "EmptyConnector", "1.0.0"));
        });

        Assert.Equal([Name], app.Loader.Runtimes.Select(r => r.Name));
    }

    [Fact]
    public void ResolveActiveDirectory_prefers_version_folders_over_assemblies_in_the_connector_folder()
    {
        // Updates install into version folders next to an existing unversioned connector; after a restart the
        // updated version must still be the one loaded.
        var root = TestConnectorApp.CreateConnectorsFolder();
        var connectorDirectory = TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name));
        var update = TestConnectorApp.CopyConnectorTo(Path.Combine(connectorDirectory, "2.0.0"));

        Assert.Equal(update, ConnectorLoader.ResolveActiveDirectory(connectorDirectory));
    }

    [Fact]
    public void ResolveActiveDirectory_uses_the_connector_folder_itself_when_it_has_no_version_folders()
    {
        var connectorDirectory = TestConnectorApp.CopyConnectorTo(Path.Combine(TestConnectorApp.CreateConnectorsFolder(), Name));

        Assert.Equal(connectorDirectory, ConnectorLoader.ResolveActiveDirectory(connectorDirectory));
    }

    [Fact]
    public void ResolveActiveDirectory_ignores_non_version_and_empty_version_folders()
    {
        var connectorDirectory = Path.Combine(TestConnectorApp.CreateConnectorsFolder(), Name);
        var expected = TestConnectorApp.CopyConnectorTo(Path.Combine(connectorDirectory, "1.5.0"));
        TestConnectorApp.CopyConnectorTo(Path.Combine(connectorDirectory, "latest"));
        Directory.CreateDirectory(Path.Combine(connectorDirectory, "3.0.0"));

        Assert.Equal(expected, ConnectorLoader.ResolveActiveDirectory(connectorDirectory));
    }

    [Fact]
    public void ResolveActiveDirectory_returns_null_when_nothing_can_be_loaded()
    {
        var connectorDirectory = Directory.CreateDirectory(Path.Combine(TestConnectorApp.CreateConnectorsFolder(), Name)).FullName;

        Assert.Null(ConnectorLoader.ResolveActiveDirectory(connectorDirectory));
    }

    [Fact]
    public void ResolveType_finds_app_and_connector_types()
    {
        using var app = TestConnectorApp.Create();

        Assert.Same(typeof(ConnectorLoader), app.Loader.ResolveType(typeof(ConnectorLoader).FullName!));
        Assert.Same(app.ConnectorType("Widget"), app.Loader.ResolveType($"{TestConnectorApp.Namespace}.Widget"));
        Assert.Null(app.Loader.ResolveType("Does.Not.Exist"));
    }

    [Fact]
    public void FindRuntime_finds_the_connector_behind_generic_arguments_and_arrays()
    {
        using var app = TestConnectorApp.Create();
        var widget = app.ConnectorType("Widget");

        Assert.Null(app.Loader.FindRuntime(typeof(string)));
        Assert.Null(app.Loader.FindRuntime(typeof(List<string>)));
        Assert.Same(app.Runtime, app.Loader.FindRuntime(widget));
        Assert.Same(app.Runtime, app.Loader.FindRuntime(typeof(List<>).MakeGenericType(widget)));
        Assert.Same(app.Runtime, app.Loader.FindRuntime(widget.MakeArrayType()));
        Assert.Same(app.Runtime, app.Loader.FindRuntime(widget.Assembly));
    }

    [Fact]
    public void LoadConnector_rejects_a_connector_that_is_already_loaded()
    {
        using var app = TestConnectorApp.Create();

        var error = Assert.Throws<InvalidOperationException>(() => app.Loader.LoadConnector(Path.Combine(app.ConnectorsPath, Name)));
        Assert.Contains("already loaded", error.Message);
    }

    [Fact]
    public void LoadConnector_rejects_conflicting_registrations_and_keeps_the_catalog_unchanged()
    {
        using var app = TestConnectorApp.Create();
        var before = app.Loader.Runtimes;

        // The same assembly under another connector name collides with the loaded one.
        var copy = TestConnectorApp.CopyConnectorTo(Path.Combine(app.ConnectorsPath, "Another.Connector"));

        Assert.Throws<ArgumentException>(() => app.Loader.LoadConnector(copy));
        Assert.Same(before, app.Loader.Runtimes);
    }

    [Fact]
    public async Task Unload_and_replace_match_connector_names_exactly()
    {
        using var app = TestConnectorApp.Create();

        await Assert.ThrowsAsync<InvalidOperationException>(() => app.Loader.UnloadConnectorAsync("EdNexusData.Broker.Connector"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => app.Loader.ReplaceConnectorAsync(Name.ToLowerInvariant(), app.CreateNewVersionFolder()));
        Assert.Single(app.Loader.Runtimes);
    }

    [Fact]
    public async Task Replace_keeps_the_old_version_when_the_new_one_fails_to_load()
    {
        using var app = TestConnectorApp.Create();
        var before = app.Runtime;
        var broken = Directory.CreateDirectory(Path.Combine(app.ConnectorsPath, Name, "2.0.0")).FullName;
        File.WriteAllText(Path.Combine(broken, "NotAnAssembly.dll"), "not a .NET assembly");

        await Assert.ThrowsAsync<InvalidOperationException>(() => app.Loader.ReplaceConnectorAsync(Name, broken));

        Assert.Same(before, app.Runtime);
        Assert.False(before.IsRetired);
    }

    [Fact]
    public async Task Unload_takes_the_connector_out_of_service_and_releases_it()
    {
        using var app = TestConnectorApp.Create();
        var typeName = $"{TestConnectorApp.Namespace}.Widget";

        var result = await app.Loader.UnloadConnectorAsync(Name);

        Assert.Empty(app.Loader.Runtimes);
        Assert.Empty(app.Loader.Connectors);
        Assert.Null(app.Loader.ResolveType(typeName));
        Assert.Equal(new ConnectorUnloadResult(Name, new Version(1, 0, 0, 0), Collected: true), result);
    }

    [Fact]
    public async Task Replace_puts_the_new_version_in_service_and_releases_the_old_one()
    {
        using var app = TestConnectorApp.Create();
        var newVersion = app.CreateNewVersionFolder();
        var oldContextId = ContextIdOf(app);

        var result = await app.Loader.ReplaceConnectorAsync(Name, newVersion);

        Assert.True(result.Collected);
        Assert.Equal(newVersion, app.Runtime.Directory);
        Assert.NotEqual(oldContextId, ContextIdOf(app));
        Assert.True(ResolveTypeReturnsCurrentVersion(app));
    }

    [Fact]
    public async Task ResolveType_never_returns_a_version_that_is_waiting_to_unload()
    {
        using var app = TestConnectorApp.Create();
        var scope = app.Services.CreateAsyncScope();
        UseConnectorIn(scope, app);

        // The old version stays loaded until the scope ends, but lookups must already see the new one.
        var retirement = app.Loader.ReplaceConnectorAsync(Name, app.CreateNewVersionFolder());

        Assert.True(ResolveTypeReturnsCurrentVersion(app));
        await scope.DisposeAsync();
        Assert.True((await retirement.WaitAsync(TimeSpan.FromSeconds(30))).Collected);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ContextIdOf(TestConnectorApp app) => RuntimeHelpers.GetHashCode(app.Runtime.LoadContext);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ResolveTypeReturnsCurrentVersion(TestConnectorApp app)
    {
        var type = app.Loader.ResolveType($"{TestConnectorApp.Namespace}.Widget");
        return type is not null && AssemblyLoadContext.GetLoadContext(type.Assembly) == app.Runtime.LoadContext;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void UseConnectorIn(AsyncServiceScope scope, TestConnectorApp app) =>
        ConnectorActivator.CreateInstance(scope.ServiceProvider, app.ConnectorType("ActivatedJob"));
}
