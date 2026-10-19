using EdNexusData.Broker.Core.Models;

namespace EdNexusData.Broker.Core.Tests.Unit.Updater;

public class StoreConnectorTests
{
    private static readonly string Hash = new('a', 64);

    [Theory]
    [InlineData("0.0.20", "0.0.20.0")]
    [InlineData("v1.2", "1.2.0.0")]
    [InlineData("1.2.3.4", "1.2.3.4")]
    public void ParseVersion_treats_missing_components_as_zero(string input, string expected)
    {
        Assert.Equal(Version.Parse(expected), StoreConnectorRelease.ParseVersion(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("edge")]
    [InlineData("latest")]
    public void ParseVersion_returns_null_for_non_versions(string? input)
    {
        Assert.Null(StoreConnectorRelease.ParseVersion(input));
    }

    [Fact]
    public void LatestRelease_orders_by_version_not_text_and_ignores_unparseable_versions()
    {
        var connector = Connector("0.0.9", "0.0.10", "not-a-version", "0.0.2");

        Assert.Equal("0.0.10", connector.LatestRelease()!.Version);
    }

    [Fact]
    public void LatestRelease_is_null_without_releases()
    {
        Assert.Null(new StoreConnector { Releases = [] }.LatestRelease());
        Assert.Null(new StoreConnector().LatestRelease());
    }

    [Theory]
    [InlineData("*", "2.0.0")]
    [InlineData("", "2.0.0")]
    [InlineData(null, "2.0.0")]
    [InlineData("1.0.0", "1.0.0")]
    [InlineData("1.0", "1.0.0")]
    public void FindRelease_follows_latest_for_star_and_pins_other_versions(string? wanted, string expected)
    {
        Assert.Equal(expected, Connector("1.0.0", "2.0.0").FindRelease(wanted)!.Version);
    }

    [Fact]
    public void FindRelease_is_null_for_a_version_the_store_does_not_have()
    {
        Assert.Null(Connector("1.0.0").FindRelease("3.0.0"));
    }

    [Fact]
    public void PackageFor_prefers_the_most_specific_platform()
    {
        var release = new StoreConnectorRelease
        {
            Packages =
            [
                new() { Platform = "any", Url = "https://x/any", Sha256 = Hash },
                new() { Platform = "linux-x64", Url = "https://x/x64", Sha256 = Hash },
                new() { Platform = "linux-amd64", Url = "https://x/amd64", Sha256 = Hash },
            ],
        };

        Assert.Equal("https://x/amd64", release.PackageFor(["linux-amd64", "linux-x64"])!.Url);
        Assert.Equal("https://x/x64", release.PackageFor(["linux-x64"])!.Url);
        Assert.Equal("https://x/any", release.PackageFor(["linux-arm64"])!.Url);
    }

    [Fact]
    public void PackageFor_never_chooses_a_package_without_a_checksum()
    {
        var release = new StoreConnectorRelease
        {
            Packages = [new() { Platform = "linux-amd64", Url = "https://x/amd64" }],
        };

        Assert.Null(release.PackageFor(["linux-amd64"]));
    }

    [Fact]
    public void PackageFor_ignores_todays_source_archive_urls_that_have_no_checksum()
    {
        // The store currently lists GitHub source archives like this; they are not installable.
        var release = new StoreConnectorRelease
        {
            Version = "0.0.1",
            Url = "https://github.com/ednexusdata/broker-connector-edupointsynergy/archive/refs/tags/v0.0.1.zip",
        };

        Assert.Null(release.PackageFor(["linux-amd64"]));
    }

    [Fact]
    public void PackageFor_accepts_a_release_level_url_with_a_checksum_as_platform_independent()
    {
        var release = new StoreConnectorRelease { Url = "https://x/package.tar.gz", Sha256 = Hash };

        Assert.Equal("https://x/package.tar.gz", release.PackageFor(["linux-arm64"])!.Url);
    }

    private static StoreConnector Connector(params string[] versions) => new()
    {
        ReferenceName = "Test.Connector",
        Releases = versions.Select(v => new StoreConnectorRelease { Version = v }).ToList(),
    };
}
