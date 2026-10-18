using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EdNexusData.Broker.Core.Security;

namespace EdNexusData.Broker.Core.Security.Tests.Unit;

public class PfxCertificateLoaderTests : IDisposable
{
    private readonly List<string> tempFiles = new();

    private string CreateTempPfx(string? exportPassword)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=broker-test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddMinutes(5));

        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pfx");
        File.WriteAllBytes(path, cert.Export(X509ContentType.Pkcs12, exportPassword));
        tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var file in tempFiles)
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void Load_WithMatchingPassword_LoadsTheCertificate()
    {
        var path = CreateTempPfx("hunter2");

        var certificate = PfxCertificateLoader.Load(path, "hunter2");

        Assert.Equal("CN=broker-test", certificate.Subject);
    }

    [Fact]
    public void Load_WithWrongPassword_ThrowsCryptographicException()
    {
        var path = CreateTempPfx("hunter2");

        Assert.Throws<CryptographicException>(() => PfxCertificateLoader.Load(path, "wrong-password"));
    }

    [Fact]
    public void Load_WithEmptyPassword_LoadsAPasswordlessPfx()
    {
        // Regression test: every shipped appsettings.json configures PfxCertPassword as "" (not the
        // literal string "null"), and the PFX it pairs with has no password. Before the fix, this
        // fell through to the password-bearing branch and passed "" instead of null.
        var path = CreateTempPfx(exportPassword: null);

        var certificate = PfxCertificateLoader.Load(path, "");

        Assert.Equal("CN=broker-test", certificate.Subject);
    }

    [Fact]
    public void Load_WithLiteralNullStringPassword_LoadsAPasswordlessPfx()
    {
        // The pre-existing convention (typing the word "null") must keep working for anyone relying on it.
        var path = CreateTempPfx(exportPassword: null);

        var certificate = PfxCertificateLoader.Load(path, "null");

        Assert.Equal("CN=broker-test", certificate.Subject);
    }

    [Fact]
    public void Load_WithMissingConfigValuePassword_LoadsAPasswordlessPfx()
    {
        // IConfiguration returns null (not "") when the key is absent entirely.
        var path = CreateTempPfx(exportPassword: null);

        var certificate = PfxCertificateLoader.Load(path, null);

        Assert.Equal("CN=broker-test", certificate.Subject);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Load_Throws_WhenPathIsMissing(string? path)
    {
        Assert.Throws<InvalidOperationException>(() => PfxCertificateLoader.Load(path, "irrelevant"));
    }
}
