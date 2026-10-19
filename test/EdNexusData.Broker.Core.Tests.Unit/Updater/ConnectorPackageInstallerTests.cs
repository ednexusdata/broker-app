using System.Formats.Tar;
using System.IO.Compression;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Tests.Unit.Connector;
using EdNexusData.Broker.Core.Tests.Unit.TestHelpers;
using EdNexusData.Broker.Core.Updaters;
using File = System.IO.File;

namespace EdNexusData.Broker.Core.Tests.Unit.Updater;

[Collection(ConnectorCollection.Name)]
public class ConnectorPackageInstallerTests
{
    private const string Name = TestConnectorApp.ConnectorName;
    private static readonly Version Version2 = new(2, 0, 0, 0);

    [Fact]
    public async Task Installs_a_verified_package_into_its_version_folder()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");

        var directory = await store.CreateInstaller(app.Loader).InstallAsync(Name, Version2, package);

        Assert.Equal(Path.Combine(app.ConnectorsPath, Name, "2.0.0"), directory);
        Assert.True(File.Exists(Path.Combine(directory, $"{Name}.dll")));
        AssertNoTemporaryFiles(app);
    }

    [Fact]
    public async Task Accepts_checksums_in_the_format_GitHub_reports()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");
        package.Sha256 = "sha256:" + package.Sha256!.ToUpperInvariant();

        var directory = await store.CreateInstaller(app.Loader).InstallAsync(Name, Version2, package);

        Assert.True(File.Exists(Path.Combine(directory, $"{Name}.dll")));
    }

    [Fact]
    public async Task Installs_zip_packages_too()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var zip = BuildZip();
        store.Serve("https://downloads.example.test/package.zip", zip);
        var package = new StoreConnectorPackage { Url = "https://downloads.example.test/package.zip", Sha256 = FakeConnectorStore.Sha256Of(zip) };

        var directory = await store.CreateInstaller(app.Loader).InstallAsync(Name, Version2, package);

        Assert.True(File.Exists(Path.Combine(directory, $"{Name}.dll")));
    }

    [Fact]
    public async Task Reuses_a_version_already_installed_without_downloading()
    {
        // e.g. installed by another process sharing the connectors folder
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");
        var existing = app.CreateNewVersionFolder("2.0.0");

        var directory = await store.CreateInstaller(app.Loader).InstallAsync(Name, Version2, package);

        Assert.Equal(existing, directory);
        Assert.Equal(0, store.DownloadCount(package.Url!));
    }

    [Fact]
    public async Task Refuses_a_package_whose_checksum_does_not_match()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");
        package.Sha256 = new string('0', 64);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => store.CreateInstaller(app.Loader).InstallAsync(Name, Version2, package));

        Assert.Contains("failed verification", error.Message);
        Assert.False(Directory.Exists(Path.Combine(app.ConnectorsPath, Name, "2.0.0")));
        AssertNoTemporaryFiles(app);
    }

    [Theory]
    [InlineData("http://downloads.example.test/package.tar.gz")]
    [InlineData("file:///tmp/package.tar.gz")]
    [InlineData("package.tar.gz")]
    public async Task Refuses_package_urls_that_are_not_https(string url)
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = new StoreConnectorPackage { Url = url, Sha256 = new string('a', 64) };

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.CreateInstaller(app.Loader).InstallAsync(Name, Version2, package));
        Assert.Empty(store.Requests);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-hex-not-hex-not-hex-not-hex-not-hex-not-hex-not-hex-not-hex!")]
    [InlineData("abc123")]
    public async Task Refuses_packages_without_a_valid_checksum(string sha256)
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");
        package.Sha256 = sha256;

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.CreateInstaller(app.Loader).InstallAsync(Name, Version2, package));
        Assert.Equal(0, store.DownloadCount(package.Url!));
    }

    [Theory]
    [InlineData("../Escaped")]
    [InlineData("Some/Connector")]
    [InlineData(".hidden")]
    public async Task Refuses_connector_names_that_are_not_plain_folder_names(string name)
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");

        await Assert.ThrowsAsync<InvalidOperationException>(() => store.CreateInstaller(app.Loader).InstallAsync(name, Version2, package));
    }

    [Fact]
    public async Task Refuses_a_package_that_does_not_contain_the_connector_assembly()
    {
        // e.g. the store listing a connector under a name that does not match its assembly
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.CreateInstaller(app.Loader).InstallAsync("EdNexusData.Broker.Connectors.TestConnector", Version2, package));

        Assert.Contains("does not contain EdNexusData.Broker.Connectors.TestConnector.dll", error.Message);
        Assert.False(Directory.Exists(Path.Combine(app.ConnectorsPath, "EdNexusData.Broker.Connectors.TestConnector", "2.0.0")));
    }

    [Fact]
    public async Task Refuses_archive_entries_that_escape_the_install_folder()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var archive = BuildTarGzWithEntry("../../escaped.dll");
        store.Serve("https://downloads.example.test/evil.tar.gz", archive);
        var package = new StoreConnectorPackage { Url = "https://downloads.example.test/evil.tar.gz", Sha256 = FakeConnectorStore.Sha256Of(archive) };

        await Assert.ThrowsAnyAsync<IOException>(() => store.CreateInstaller(app.Loader).InstallAsync(Name, Version2, package));

        Assert.False(File.Exists(Path.Combine(app.ConnectorsPath, "escaped.dll")));
        Assert.False(File.Exists(Path.Combine(Path.GetDirectoryName(app.ConnectorsPath)!, "escaped.dll")));
        AssertNoTemporaryFiles(app);
    }

    [Fact]
    public async Task Refuses_packages_over_the_size_limit()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var package = store.Publish("2.0.0");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            store.CreateInstaller(app.Loader, maxPackageMegabytes: 0).InstallAsync(Name, Version2, package));

        Assert.Contains("MB limit", error.Message);
        AssertNoTemporaryFiles(app);
    }

    [Fact]
    public async Task Refuses_files_that_are_not_archives()
    {
        using var app = TestConnectorApp.Create();
        var store = new FakeConnectorStore();
        var content = "<html>not found</html>"u8.ToArray();
        store.Serve("https://downloads.example.test/page.html", content);
        var package = new StoreConnectorPackage { Url = "https://downloads.example.test/page.html", Sha256 = FakeConnectorStore.Sha256Of(content) };

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => store.CreateInstaller(app.Loader).InstallAsync(Name, Version2, package));

        Assert.Contains("neither a .tar.gz nor a .zip", error.Message);
    }

    [Fact]
    public void Current_platforms_use_the_publish_workflow_names_first()
    {
        var platforms = ConnectorPackageInstaller.CurrentPlatforms;

        Assert.NotEmpty(platforms);
        if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.X64)
        {
            Assert.EndsWith("-amd64", platforms[0]);
            Assert.EndsWith("-x64", platforms[1]);
        }
    }

    private static void AssertNoTemporaryFiles(TestConnectorApp app)
    {
        var connectorDirectory = Path.Combine(app.ConnectorsPath, Name);
        Assert.Empty(Directory.EnumerateFileSystemEntries(connectorDirectory, ".download-*"));
        Assert.Empty(Directory.EnumerateFileSystemEntries(connectorDirectory, ".staging-*"));
    }

    private static byte[] BuildZip()
    {
        var root = TestConnectorApp.CreateConnectorsFolder();
        TestConnectorApp.CopyConnectorTo(Path.Combine(root, "package"));
        var zipPath = Path.Combine(TestConnectorApp.CreateConnectorsFolder(), "package.zip");
        ZipFile.CreateFromDirectory(Path.Combine(root, "package"), zipPath, CompressionLevel.Fastest, includeBaseDirectory: true);
        return File.ReadAllBytes(zipPath);
    }

    private static byte[] BuildTarGzWithEntry(string entryName)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true))
        using (var tar = new TarWriter(gzip))
        {
            var entry = new PaxTarEntry(TarEntryType.RegularFile, entryName) { DataStream = new MemoryStream("x"u8.ToArray()) };
            tar.WriteEntry(entry);
        }
        return output.ToArray();
    }
}
