using System.Security.Cryptography;
using System.Text.Json;
using Ardalis.Specification;
using EdNexusData.Broker.Core.Security;
using EdNexusData.Broker.Core.Serializers;
using EdNexusData.Broker.Core.Tests.Unit.TestHelpers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EdNexusData.Broker.Core.Serializers.Tests.Unit;

public class ConfigurationSerializerTests
{
    private readonly Mock<IRepository<EducationOrganizationConnectorSettings>> repo = new();
    private readonly IDataProtectionProvider dataProtectionProvider = new EphemeralDataProtectionProvider();
    private readonly ConfigurationSerializer serializer;

    private EducationOrganizationConnectorSettings? saved;

    public ConfigurationSerializerTests()
    {
        repo.Setup(r => r.AddAsync(It.IsAny<EducationOrganizationConnectorSettings>(), It.IsAny<CancellationToken>()))
            .Callback<EducationOrganizationConnectorSettings, CancellationToken>((entity, _) => saved = entity)
            .ReturnsAsync((EducationOrganizationConnectorSettings entity, CancellationToken _) => entity);

        repo.Setup(r => r.UpdateAsync(It.IsAny<EducationOrganizationConnectorSettings>(), It.IsAny<CancellationToken>()))
            .Callback<EducationOrganizationConnectorSettings, CancellationToken>((entity, _) => saved = entity)
            .Returns(Task.CompletedTask);

        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        serializer = new ConfigurationSerializer(repo.Object, dataProtectionProvider, serviceProvider);
    }

    [Fact]
    public async Task SerializeAndSaveAsync_ThenDeseralizeAsync_RoundTripsCredentials()
    {
        var orgId = Guid.NewGuid();
        var config = new FakeConnectorConfiguration { Username = "svc-account", Password = "sup3r-secret" };

        await serializer.SerializeAndSaveAsync(config, orgId);
        Assert.NotNull(saved);

        repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<EducationOrganizationConnectorSettings>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(saved);

        var result = await serializer.DeseralizeAsync(typeof(FakeConnectorConfiguration), orgId);

        var fake = Assert.IsType<FakeConnectorConfiguration>(result);
        Assert.Equal("svc-account", fake.Username);
        Assert.Equal("sup3r-secret", fake.Password);
    }

    [Fact]
    public async Task SerializeAndSaveAsync_EncryptsWithBoundPurpose_NotTheLegacyPurpose()
    {
        var orgId = Guid.NewGuid();
        var config = new FakeConnectorConfiguration { Username = "svc-account", Password = "sup3r-secret" };

        await serializer.SerializeAndSaveAsync(config, orgId);
        Assert.NotNull(saved);

        var wrapper = JsonSerializer.Deserialize<Dictionary<string, string>>(saved!.Settings!.RootElement.GetRawText())!;
        var ciphertext = wrapper[typeof(FakeConnectorConfiguration).FullName!];

        // Newly written settings must NOT be readable via the pre-hardening unbound purpose -
        // otherwise the binding fix would only apply to reads, not writes.
        var legacyProtector = ConnectorSettingsProtector.CreateLegacyProtector(dataProtectionProvider);
        Assert.Throws<CryptographicException>(() => legacyProtector.Unprotect(ciphertext));
    }

    [Fact]
    public async Task DeseralizeAsync_FallsBackToLegacyPurpose_ForSettingsEncryptedBeforeBindingExisted()
    {
        var orgId = Guid.NewGuid();
        var connectorName = typeof(FakeConnectorConfiguration).Assembly.GetName().Name!;

        // Simulate a row written before per-record purpose binding existed.
        var legacyProtector = ConnectorSettingsProtector.CreateLegacyProtector(dataProtectionProvider);
        var ciphertext = legacyProtector.Protect(JsonSerializer.Serialize(new { Username = "legacy-user", Password = "legacy-pass" }));

        var wrapper = new Dictionary<string, string> { [typeof(FakeConnectorConfiguration).FullName!] = ciphertext };
        var legacyRow = new EducationOrganizationConnectorSettings
        {
            EducationOrganizationId = orgId,
            Connector = connectorName,
            Settings = JsonSerializer.SerializeToDocument(wrapper)
        };

        repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<EducationOrganizationConnectorSettings>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(legacyRow);

        var result = await serializer.DeseralizeAsync(typeof(FakeConnectorConfiguration), orgId);

        var fake = Assert.IsType<FakeConnectorConfiguration>(result);
        Assert.Equal("legacy-user", fake.Username);
        Assert.Equal("legacy-pass", fake.Password);
    }

    [Fact]
    public async Task DeseralizeAsync_Throws_WhenSettingsWereEncryptedForADifferentEducationOrganization()
    {
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();
        var config = new FakeConnectorConfiguration { Username = "org-a-user", Password = "org-a-pass" };

        await serializer.SerializeAndSaveAsync(config, orgA);
        Assert.NotNull(saved);

        // Same ciphertext blob served back, but requested under a different org - this is the
        // scenario the purpose-binding fix exists to prevent (e.g. a row copied/relinked between
        // education organizations should not silently decrypt).
        repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<EducationOrganizationConnectorSettings>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(saved);

        await Assert.ThrowsAsync<CryptographicException>(
            () => serializer.DeseralizeAsync(typeof(FakeConnectorConfiguration), orgB));
    }
}
