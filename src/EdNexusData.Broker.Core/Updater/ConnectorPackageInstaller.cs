using System.Formats.Tar;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using EdNexusData.Broker.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using File = System.IO.File;

namespace EdNexusData.Broker.Core.Updaters;

/// <summary>
/// Downloads a connector package, verifies its SHA-256, and installs it as connectors/&lt;name&gt;/&lt;version&gt;/.
/// Packages are the .tar.gz (or .zip) archives the connectors' publish workflow attaches to GitHub releases.
/// The version folder appears in one atomic move, so another process sharing the folder never sees it half-written.
/// </summary>
public partial class ConnectorPackageInstaller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ConnectorLoader _connectorLoader;
    private readonly ILogger<ConnectorPackageInstaller> _logger;
    private readonly long _maxPackageBytes;

    public ConnectorPackageInstaller(
        IHttpClientFactory httpClientFactory,
        ConnectorLoader connectorLoader,
        IConfiguration configuration,
        ILogger<ConnectorPackageInstaller> logger)
    {
        _httpClientFactory = httpClientFactory;
        _connectorLoader = connectorLoader;
        _logger = logger;
        _maxPackageBytes = configuration.GetValue("ConnectorUpdates:MaxPackageMegabytes", 500L) * 1024 * 1024;
    }

    private readonly System.Collections.Concurrent.ConcurrentDictionary<(string Name, Version Version, string? Sha256), string> _failures = new();

    /// <summary>
    /// Remembers that a package could not be installed (failed verification, or would not load), so it is not
    /// downloaded again every check. A different package for the same version (new checksum) is tried again.
    /// </summary>
    public void RememberFailure(string connectorName, Version version, StoreConnectorPackage package, string reason) =>
        _failures[(connectorName, version, NormalizeHash(package.Sha256))] = reason;

    public string? PreviousFailure(string connectorName, Version version, StoreConnectorPackage package) =>
        _failures.TryGetValue((connectorName, version, NormalizeHash(package.Sha256)), out var reason) ? reason : null;

    /// <summary>Platform names this machine accepts, most specific first, e.g. linux-amd64 then linux-x64.</summary>
    public static IReadOnlyList<string> CurrentPlatforms { get; } = DetectPlatforms();

    /// <summary>Folder name for a version: 1.2.3 (or 1.2.3.4 when it has a revision).</summary>
    public static string VersionFolderName(Version version) =>
        version.Revision > 0 ? version.ToString(4) : version.ToString(3);

    /// <summary>Installs the package and returns its version folder. Reuses the folder if it is already installed.</summary>
    public async Task<string> InstallAsync(string connectorName, Version version, StoreConnectorPackage package, CancellationToken cancellationToken = default)
    {
        if (!SafeName().IsMatch(connectorName))
        {
            throw new InvalidOperationException($"'{connectorName}' is not a valid connector name.");
        }

        if (!Uri.TryCreate(package.Url, UriKind.Absolute, out var url) || url.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException($"Connector package URL must be an absolute https URL: {package.Url}");
        }

        var expectedHash = NormalizeHash(package.Sha256)
            ?? throw new InvalidOperationException($"Connector package {url} has no valid SHA-256.");

        var connectorDirectory = Path.Combine(_connectorLoader.ConnectorsPath, connectorName);
        var versionDirectory = Path.Combine(connectorDirectory, VersionFolderName(version));
        if (IsInstalled(versionDirectory, connectorName))
        {
            _logger.LogInformation("Connector {Name} {Version} is already installed in {Directory}.", connectorName, version, versionDirectory);
            return versionDirectory;
        }

        Directory.CreateDirectory(connectorDirectory);
        var id = Guid.NewGuid().ToString("N");
        var downloadPath = Path.Combine(connectorDirectory, $".download-{id}");
        var stagingPath = Path.Combine(connectorDirectory, $".staging-{id}");

        try
        {
            _logger.LogInformation("Downloading connector {Name} {Version} from {Url}.", connectorName, version, url);
            var actualHash = await DownloadAsync(url, downloadPath, cancellationToken);
            if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Connector package {url} failed verification: SHA-256 is {actualHash}, expected {expectedHash}.");
            }

            Directory.CreateDirectory(stagingPath);
            await ExtractAsync(downloadPath, stagingPath, cancellationToken);

            var payload = FindPayloadDirectory(stagingPath);
            if (!File.Exists(Path.Combine(payload, $"{connectorName}.dll")))
            {
                throw new InvalidOperationException($"Connector package {url} does not contain {connectorName}.dll.");
            }

            try
            {
                Directory.Move(payload, versionDirectory);
            }
            catch (IOException) when (IsInstalled(versionDirectory, connectorName))
            {
                // Another process sharing the connectors folder installed the same version first.
            }

            _logger.LogInformation("Installed connector {Name} {Version} into {Directory}.", connectorName, version, versionDirectory);
            return versionDirectory;
        }
        finally
        {
            TryDelete(downloadPath);
            TryDelete(stagingPath);
        }
    }

    private async Task<string> DownloadAsync(Uri url, string path, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength > _maxPackageBytes)
        {
            throw new InvalidOperationException($"Connector package {url} is larger than the {_maxPackageBytes / 1024 / 1024} MB limit.");
        }

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destination = File.Create(path);

        var buffer = new byte[81920];
        long total = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            total += read;
            if (total > _maxPackageBytes)
            {
                throw new InvalidOperationException($"Connector package {url} is larger than the {_maxPackageBytes / 1024 / 1024} MB limit.");
            }

            hash.AppendData(buffer, 0, read);
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static async Task ExtractAsync(string packagePath, string destination, CancellationToken cancellationToken)
    {
        var header = new byte[2];
        await using (var probe = File.OpenRead(packagePath))
        {
            await probe.ReadExactlyAsync(header, cancellationToken);
        }

        // Both extractors refuse entries (and links) that would land outside the destination.
        if (header[0] == 0x1F && header[1] == 0x8B)
        {
            await using var file = File.OpenRead(packagePath);
            await using var gzip = new GZipStream(file, CompressionMode.Decompress);
            await TarFile.ExtractToDirectoryAsync(gzip, destination, overwriteFiles: false, cancellationToken);
        }
        else if (header[0] == (byte)'P' && header[1] == (byte)'K')
        {
            ZipFile.ExtractToDirectory(packagePath, destination, overwriteFiles: false);
        }
        else
        {
            throw new InvalidOperationException("Connector package is neither a .tar.gz nor a .zip archive.");
        }
    }

    /// <summary>Archives from the publish workflow wrap everything in one top-level folder.</summary>
    private static string FindPayloadDirectory(string stagingPath)
    {
        var directories = Directory.GetDirectories(stagingPath);
        var hasFiles = Directory.EnumerateFiles(stagingPath).Any();
        return !hasFiles && directories.Length == 1 ? directories[0] : stagingPath;
    }

    private static bool IsInstalled(string versionDirectory, string connectorName) =>
        File.Exists(Path.Combine(versionDirectory, $"{connectorName}.dll"));

    private static string? NormalizeHash(string? hash)
    {
        var value = hash?.Trim();
        if (value is not null && value.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)) value = value["sha256:".Length..];
        return value is not null && Sha256Hex().IsMatch(value) ? value.ToLowerInvariant() : null;
    }

    private void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
            else if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Could not delete temporary connector file {Path}.", path);
        }
    }

    private static List<string> DetectPlatforms()
    {
        var os = OperatingSystem.IsWindows() ? "win" : OperatingSystem.IsMacOS() ? "osx" : "linux";
        return RuntimeInformation.OSArchitecture switch
        {
            // The publish workflow names platforms like Docker (amd64); .NET calls the same thing x64.
            Architecture.X64 => [$"{os}-amd64", $"{os}-x64"],
            Architecture.Arm64 => [$"{os}-arm64"],
            var other => [$"{os}-{other.ToString().ToLowerInvariant()}"],
        };
    }

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]*$")]
    private static partial Regex SafeName();

    [GeneratedRegex("^[0-9a-fA-F]{64}$")]
    private static partial Regex Sha256Hex();
}
