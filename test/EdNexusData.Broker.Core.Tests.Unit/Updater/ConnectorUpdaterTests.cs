using System.Runtime.CompilerServices;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Tests.Unit.Connector;
using EdNexusData.Broker.Core.Tests.Unit.TestHelpers;
using EdNexusData.Broker.Core.Updaters;
using File = System.IO.File;
using Microsoft.Extensions.DependencyInjection;

namespace EdNexusData.Broker.Core.Tests.Unit.Updater;

[Collection(ConnectorCollection.Name)]
public class ConnectorUpdaterTests
{
    private const string Name = TestConnectorApp.ConnectorName;

    [Fact]
    public async Task Updates_a_running_connector_to_the_latest_release()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        store.Publish("1.0.0");
        store.Publish("2.0.0");
        store.Reference("*");

        var result = Assert.Single(await store.CreateUpdater(app.Loader).Synchronize());

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(ConnectorUpdateAction.Update, result.Plan.Action);
        Assert.Equal(new Version(1, 0, 0, 0), result.Plan.InstalledVersion);
        Assert.Equal(new Version(2, 0, 0, 0), result.Plan.TargetVersion);
        Assert.Equal(Path.Combine(app.ConnectorsPath, Name, "2.0.0"), app.Runtime.Directory);
        Assert.Equal(new Version(2, 0, 0, 0), ConnectorUpdater.InstalledVersion(app.Runtime));
    }

    [Fact]
    public async Task Update_does_not_wait_for_work_using_the_old_version()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        store.Publish("2.0.0");
        store.Reference("*");
        var request = app.Services.CreateAsyncScope();
        UseConnectorIn(request, app);

        var result = Assert.Single(await store.CreateUpdater(app.Loader).Synchronize().WaitAsync(TimeSpan.FromSeconds(30)));

        Assert.True(result.Succeeded, result.Message);
        Assert.EndsWith("2.0.0", app.Runtime.Directory);
        await request.DisposeAsync();
    }

    [Fact]
    public async Task Does_nothing_when_the_running_version_is_the_latest()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("1.0.0");
        store.Reference("*");
        var before = app.Runtime;

        var result = Assert.Single(await store.CreateUpdater(app.Loader).Synchronize());

        Assert.Equal(ConnectorUpdateAction.None, result.Plan.Action);
        Assert.Contains("Up to date", result.Message);
        Assert.Same(before, app.Runtime);
        Assert.Equal(0, store.DownloadCount(package.Url!));
    }

    [Fact]
    public async Task Does_not_downgrade_a_connector_that_follows_the_latest_release()
    {
        using var app = TestConnectorApp.Create(root => TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name, "3.0.0")));
        var store = new FakeConnectorStore();
        store.Publish("2.0.0");
        store.Reference("*");

        var result = Assert.Single(await store.CreateUpdater(app.Loader).Synchronize());

        Assert.Equal(ConnectorUpdateAction.None, result.Plan.Action);
        Assert.EndsWith("3.0.0", app.Runtime.Directory);
    }

    [Fact]
    public async Task Moves_a_pinned_connector_to_exactly_its_pinned_version()
    {
        using var app = TestConnectorApp.Create(root => TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name, "3.0.0")));
        var store = new FakeConnectorStore();
        store.Publish("2.0.0");
        store.Publish("3.0.0");
        store.Publish("4.0.0");
        store.Reference("2.0.0");

        var result = Assert.Single(await store.CreateUpdater(app.Loader).Synchronize());

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(new Version(2, 0, 0, 0), result.Plan.TargetVersion);
        Assert.EndsWith("2.0.0", app.Runtime.Directory);
    }

    [Fact]
    public async Task Installs_a_referenced_connector_that_is_not_installed_yet()
    {
        using var app = TestConnectorApp.Create(_ => { });
        var store = new FakeConnectorStore();
        store.Publish("1.5.0");
        store.Reference("*");

        var result = Assert.Single(await store.CreateUpdater(app.Loader).Synchronize());

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(ConnectorUpdateAction.Install, result.Plan.Action);
        Assert.Null(result.Plan.InstalledVersion);
        Assert.Equal(Path.Combine(app.ConnectorsPath, Name, "1.5.0"), app.Runtime.Directory);
    }

    [Fact]
    public async Task Leaves_connectors_without_a_reference_alone()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        store.Publish("2.0.0");
        var before = app.Runtime;

        Assert.Empty(await store.CreateUpdater(app.Loader).Synchronize());
        Assert.Same(before, app.Runtime);
    }

    [Fact]
    public async Task Skips_releases_with_no_installable_package()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        // What the store lists today: a source archive with no checksum.
        store.AddRelease(Name, new StoreConnectorRelease { Version = "2.0.0", Url = "https://github.com/ednexusdata/example/archive/refs/tags/v2.0.0.zip" });
        store.Reference("*");

        var result = Assert.Single(await store.CreateUpdater(app.Loader).Synchronize());

        Assert.Equal(ConnectorUpdateAction.None, result.Plan.Action);
        Assert.Contains("no installable package", result.Message);
        Assert.Single(store.Requests); // only the store listing
    }

    [Theory]
    [InlineData(false, "Not listed in the connector store")]
    [InlineData(true, "no release matching version '9.9.9'")]
    public async Task Explains_why_a_reference_cannot_be_updated(bool listed, string reason)
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        if (listed) store.Publish("2.0.0");
        store.Reference(listed ? "9.9.9" : "*");

        var result = Assert.Single(await store.CreateUpdater(app.Loader).Synchronize());

        Assert.Equal(ConnectorUpdateAction.None, result.Plan.Action);
        Assert.Contains(reason, result.Message);
    }

    [Fact]
    public async Task A_failed_update_keeps_the_running_version_and_other_connectors_still_update()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var broken = store.Publish("2.0.0", connectorName: "Broken.Connector");
        broken.Sha256 = new string('0', 64);
        store.Publish("2.0.0");
        store.Reference("*", "Broken.Connector");
        store.Reference("*");

        var results = await store.CreateUpdater(app.Loader).Synchronize();

        var failed = results.Single(r => r.Plan.Name == "Broken.Connector");
        Assert.False(failed.Succeeded);
        Assert.Contains("failed verification", failed.Message);
        Assert.True(results.Single(r => r.Plan.Name == Name).Succeeded);
        Assert.EndsWith("2.0.0", app.Runtime.Directory);
        Assert.Single(app.Loader.Runtimes);
    }

    [Fact]
    public async Task A_package_that_will_not_load_is_removed_and_not_downloaded_again()
    {
        // e.g. a connector built against a different EdNexusData.Broker.Common
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var archive = FakeConnectorStore.BuildTarGz("incompatible", directory =>
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, $"{Name}.dll"), "not a .NET assembly");
            return directory;
        });
        const string url = "https://downloads.example.test/incompatible.tar.gz";
        store.Serve(url, archive);
        store.AddRelease(Name, new StoreConnectorRelease
        {
            Version = "2.0.0",
            Packages = [new StoreConnectorPackage { Url = url, Sha256 = FakeConnectorStore.Sha256Of(archive) }],
        });
        store.Reference("*");
        var before = app.Runtime;
        var updater = store.CreateUpdater(app.Loader);

        var first = Assert.Single(await updater.Synchronize());
        var second = Assert.Single(await updater.Synchronize());

        Assert.False(first.Succeeded);
        Assert.Same(before, app.Runtime);
        // Left on disk, it would be the first version tried on the next start.
        Assert.False(Directory.Exists(Path.Combine(app.ConnectorsPath, Name, "2.0.0")));
        Assert.Equal(ConnectorUpdateAction.None, second.Plan.Action);
        Assert.Contains("failed to install earlier", second.Message);
        Assert.Equal(1, store.DownloadCount(url));
    }

    [Fact]
    public async Task A_corrected_package_for_a_failed_version_is_tried_again()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");
        var correctHash = package.Sha256;
        package.Sha256 = new string('0', 64);
        store.Reference("*");
        var updater = store.CreateUpdater(app.Loader);

        Assert.False(Assert.Single(await updater.Synchronize()).Succeeded);
        package.Sha256 = correctHash;
        var retried = Assert.Single(await updater.Synchronize());

        Assert.True(retried.Succeeded, retried.Message);
        Assert.EndsWith("2.0.0", app.Runtime.Directory);
    }

    [Fact]
    public async Task Download_errors_are_retried_at_the_next_check()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        store.AddRelease(Name, new StoreConnectorRelease
        {
            Version = "2.0.0",
            Packages = [new StoreConnectorPackage { Url = "https://downloads.example.test/missing.tar.gz", Sha256 = new string('a', 64) }],
        });
        store.Reference("*");
        var updater = store.CreateUpdater(app.Loader);

        await updater.Synchronize();
        await updater.Synchronize();

        Assert.Equal(2, store.DownloadCount("https://downloads.example.test/missing.tar.gz"));
    }

    [Fact]
    public async Task Picks_the_package_for_this_platform()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var mine = ConnectorPackageInstaller.CurrentPlatforms[0];
        var other = store.Publish("2.0.0", platform: "plan9-mips");
        var matching = store.Publish("2.0.0", platform: mine);
        // Both packages belong to the same 2.0.0 release.
        var listing = store.Listing.Single();
        listing.Releases = [new StoreConnectorRelease { Version = "2.0.0", Packages = [other, matching] }];
        store.Reference("*");

        var result = Assert.Single(await store.CreateUpdater(app.Loader).Synchronize());

        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(1, store.DownloadCount(matching.Url!));
        Assert.Equal(0, store.DownloadCount(other.Url!));
    }

    [Fact]
    public async Task Running_again_after_an_update_does_nothing()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");
        store.Reference("*");
        var updater = store.CreateUpdater(app.Loader);

        await updater.Synchronize();
        var second = Assert.Single(await updater.Synchronize());

        Assert.Equal(ConnectorUpdateAction.None, second.Plan.Action);
        Assert.Equal(1, store.DownloadCount(package.Url!));
    }

    [Fact]
    public void InstalledVersion_uses_the_version_folder_and_falls_back_to_the_assembly_version()
    {
        using var unversioned = TestConnectorApp.Create();
        using var versioned = TestConnectorApp.Create(root => TestConnectorApp.CopyConnectorTo(Path.Combine(root, Name, "0.0.21")));

        Assert.Equal(new Version(1, 0, 0, 0), ConnectorUpdater.InstalledVersion(unversioned.Runtime));
        Assert.Equal(new Version(0, 0, 21, 0), ConnectorUpdater.InstalledVersion(versioned.Runtime));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void UseConnectorIn(AsyncServiceScope scope, TestConnectorApp app) =>
        ConnectorActivator.CreateInstance(scope.ServiceProvider, app.ConnectorType("ActivatedJob"));
}
