using System.Formats.Tar;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using EdNexusData.Broker.Core.Models;
using EdNexusData.Broker.Core.Services;
using EdNexusData.Broker.Core.Updaters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EdNexusData.Broker.Core.Tests.Unit.TestHelpers;

/// <summary>
/// Serves a connector store listing and connector packages over fake https URLs, and builds packages in the
/// layout the connectors' publish workflow produces (a .tar.gz with one top-level folder).
/// </summary>
public sealed class FakeConnectorStore : HttpMessageHandler, IHttpClientFactory
{
    public const string StoreUrl = "https://store.example.test/connectors";

    private readonly Dictionary<string, byte[]> _files = new();

    public List<StoreConnector> Listing { get; } = new();
    public List<ConnectorReference> References { get; } = new();
    public List<string> Requests { get; } = new();

    public int DownloadCount(string url) => Requests.Count(r => r == url);

    public void Serve(string url, byte[] content) => _files[url] = content;

    /// <summary>Publishes a package of the test connector as <paramref name="version"/> and lists it in the store.</summary>
    public StoreConnectorPackage Publish(string version, string? platform = null, string? connectorName = null)
    {
        var name = connectorName ?? TestConnectorApp.ConnectorName;
        var url = $"https://downloads.example.test/{name}-{version}-{platform ?? "any"}.tar.gz";
        var package = BuildTarGz($"{name}-v{version}-{platform ?? "any"}", TestConnectorApp.CopyConnectorTo);
        Serve(url, package);

        var storePackage = new StoreConnectorPackage { Platform = platform, Url = url, Sha256 = Sha256Of(package) };
        AddRelease(name, new StoreConnectorRelease { Version = version, Packages = [storePackage] });
        return storePackage;
    }

    public void AddRelease(string connectorName, StoreConnectorRelease release)
    {
        var connector = Listing.FirstOrDefault(c => c.ReferenceName == connectorName);
        if (connector is null)
        {
            connector = new StoreConnector { ReferenceName = connectorName, Name = connectorName, Company = "Test", Releases = [] };
            Listing.Add(connector);
        }
        connector.Releases!.Add(release);
    }

    public void Reference(string version = "*", string connectorName = TestConnectorApp.ConnectorName) =>
        References.Add(new ConnectorReference { Reference = connectorName, Version = version });

    public ConnectorPackageInstaller CreateInstaller(ConnectorLoader loader, int maxPackageMegabytes = 500) =>
        new(this, loader, Configuration(maxPackageMegabytes), NullLogger<ConnectorPackageInstaller>.Instance);

    public ConnectorUpdater CreateUpdater(ConnectorLoader loader)
    {
        var repository = new Mock<IRepository<ConnectorReference>>();
        repository.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => References.ToList());
        var connectorService = new ConnectorService(repository.Object, this, Configuration(500));

        return new ConnectorUpdater(connectorService, loader, CreateInstaller(loader), NullLogger<ConnectorUpdater>.Instance);
    }

    /// <summary>A .tar.gz whose single top-level folder <paramref name="folderName"/> is filled by <paramref name="fill"/>.</summary>
    public static byte[] BuildTarGz(string folderName, Func<string, string> fill)
    {
        var root = TestConnectorApp.CreateConnectorsFolder();
        fill(Path.Combine(root, folderName));

        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true))
        {
            TarFile.CreateFromDirectory(Path.Combine(root, folderName), gzip, includeBaseDirectory: true);
        }
        Directory.Delete(root, recursive: true);
        return output.ToArray();
    }

    public static string Sha256Of(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));

    HttpClient IHttpClientFactory.CreateClient(string name) => new(this, disposeHandler: false);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri!.ToString();
        lock (Requests) Requests.Add(url);

        if (url == StoreUrl)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Listing) });
        }

        return Task.FromResult(_files.TryGetValue(url, out var content)
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content) }
            : new HttpResponseMessage(HttpStatusCode.NotFound));
    }

    private static IConfiguration Configuration(int maxPackageMegabytes) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectorUpdates:StoreUrl"] = StoreUrl,
                ["ConnectorUpdates:MaxPackageMegabytes"] = maxPackageMegabytes.ToString(),
            })
            .Build();
}
