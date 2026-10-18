namespace EdNexusData.Broker.Core.Models;

public class StoreConnector
{
    public string? ReferenceName { get; set; }
    public string? Company { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public List<StoreConnectorRelease>? Releases { get; set; }

    public StoreConnectorRelease? LatestRelease()
    {
        return Releases?
            .Where(r => r.ToSystemVersion() is not null)
            .OrderByDescending(r => r.ToSystemVersion())
            .FirstOrDefault();
    }

    /// <summary>The release a connector reference asks for: "*" (or empty) for the latest, otherwise that exact version.</summary>
    public StoreConnectorRelease? FindRelease(string? version)
    {
        if (string.IsNullOrWhiteSpace(version) || version == "*") return LatestRelease();

        var wanted = StoreConnectorRelease.ParseVersion(version);
        return wanted is null ? null : Releases?.FirstOrDefault(r => r.ToSystemVersion() == wanted);
    }

    public ConnectorReference ToConnectorReference()
    {
        return new ConnectorReference()
        {
            Reference = ReferenceName!,
            Version = "*"
        };
    }
}

public class StoreConnectorRelease
{
    public string? Version { get; set; }

    /// <summary>
    /// A platform-independent package, installable only together with <see cref="Sha256"/>.
    /// (Older store entries point this at a source archive with no checksum; those are never installed.)
    /// </summary>
    public string? Url { get; set; }
    public string? Sha256 { get; set; }

    /// <summary>Compiled packages per platform, as produced by the connectors' publish workflow.</summary>
    public List<StoreConnectorPackage>? Packages { get; set; }

    public Version? ToSystemVersion() => ParseVersion(Version);

    /// <summary>
    /// The package to install on a platform: the first one listed for any of <paramref name="platforms"/> (most
    /// specific first), otherwise a platform-independent one. Packages without a checksum are never chosen.
    /// </summary>
    public StoreConnectorPackage? PackageFor(IReadOnlyList<string> platforms)
    {
        var packages = (Packages ?? []).Where(p => p.IsInstallable).ToList();

        foreach (var platform in platforms)
        {
            var match = packages.FirstOrDefault(p => string.Equals(p.Platform, platform, StringComparison.OrdinalIgnoreCase));
            if (match is not null) return match;
        }

        var platformIndependent = packages.FirstOrDefault(p => string.IsNullOrEmpty(p.Platform) || string.Equals(p.Platform, "any", StringComparison.OrdinalIgnoreCase));
        if (platformIndependent is not null) return platformIndependent;

        var releaseLevel = new StoreConnectorPackage { Url = Url, Sha256 = Sha256 };
        return releaseLevel.IsInstallable ? releaseLevel : null;
    }

    /// <summary>Parses "1.2.3" or "v1.2.3", treating missing components as 0 so 1.2.3 equals 1.2.3.0.</summary>
    public static Version? ParseVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version)) return null;

        var trimmed = version.Trim().TrimStart('v', 'V');
        return System.Version.TryParse(trimmed, out var parsed) ? Normalize(parsed) : null;
    }

    public static Version Normalize(Version version) =>
        new(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
}

public class StoreConnectorPackage
{
    /// <summary>e.g. linux-amd64 or linux-arm64; empty or "any" for platform-independent.</summary>
    public string? Platform { get; set; }
    public string? Url { get; set; }

    /// <summary>Hex SHA-256 of the package file, optionally prefixed "sha256:" as GitHub reports it.</summary>
    public string? Sha256 { get; set; }

    public bool IsInstallable => !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(Sha256);
}
