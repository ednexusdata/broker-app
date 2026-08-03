using System.Security.Cryptography;
using System.Text.Json;
using Ardalis.Specification;
using EdNexusData.Broker.Core.Resolvers;
using EdNexusData.Broker.Core.Security;
using EdNexusData.Broker.Core.Tests.Unit.TestHelpers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EdNexusData.Broker.Core.Resolvers.Tests.Unit;

public class ConfigurationResolverTests
{
    private readonly Mock<IRepository<EducationOrganizationConnectorSettings>> connectorSettingsRepo = new();
    private readonly IDataProtectionProvider dataProtectionProvider = new EphemeralDataProtectionProvider();
    private readonly ConfigurationResolver resolver;

    public ConfigurationResolverTests()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var edOrgRepo = new Mock<IRepository<EducationOrganization>>();
        var districtResolver = new DistrictEducationOrganizationResolver(edOrgRepo.Object, serviceProvider);
        var focusResolver = new FocusEducationOrganizationResolver(edOrgRepo.Object, serviceProvider);

        resolver = new ConfigurationResolver(
            connectorSettingsRepo.Object,
            dataProtectionProvider,
            districtResolver,
            focusResolver,
            serviceProvider);
    }

    private static JsonDocument BuildSettingsDocument(IDataProtector protector, string typeFullName, object plaintext)
    {
        var ciphertext = protector.Protect(JsonSerializer.Serialize(plaintext));
        var wrapper = new Dictionary<string, string> { [typeFullName] = ciphertext };
        return JsonSerializer.SerializeToDocument(wrapper);
    }

    [Fact]
    public async Task FetchConnectorSettingsAsync_DecryptsSettings_BoundToConnectorAndOrg()
    {
        var orgId = Guid.NewGuid();
        var connectorName = typeof(FakeConnectorConfiguration).Assembly.GetName().Name!;
        var protector = ConnectorSettingsProtector.CreateBoundProtector(dataProtectionProvider, connectorName, orgId);

        var row = new EducationOrganizationConnectorSettings
        {
            EducationOrganizationId = orgId,
            Connector = connectorName,
            Settings = BuildSettingsDocument(protector, typeof(FakeConnectorConfiguration).FullName!, new { Username = "svc", Password = "p@ss" })
        };

        connectorSettingsRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<EducationOrganizationConnectorSettings>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(row);

        var result = await resolver.FetchConnectorSettingsAsync<FakeConnectorConfiguration>(orgId);

        Assert.Equal("svc", result.Username);
        Assert.Equal("p@ss", result.Password);
    }

    [Fact]
    public async Task FetchConnectorSettingsAsync_FallsBackToLegacyPurpose_ForPreExistingSettings()
    {
        var orgId = Guid.NewGuid();
        var connectorName = typeof(FakeConnectorConfiguration).Assembly.GetName().Name!;
        var legacyProtector = ConnectorSettingsProtector.CreateLegacyProtector(dataProtectionProvider);

        var row = new EducationOrganizationConnectorSettings
        {
            EducationOrganizationId = orgId,
            Connector = connectorName,
            Settings = BuildSettingsDocument(legacyProtector, typeof(FakeConnectorConfiguration).FullName!, new { Username = "legacy", Password = "legacy-pass" })
        };

        connectorSettingsRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<EducationOrganizationConnectorSettings>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(row);

        var result = await resolver.FetchConnectorSettingsAsync<FakeConnectorConfiguration>(orgId);

        Assert.Equal("legacy", result.Username);
        Assert.Equal("legacy-pass", result.Password);
    }

    [Fact]
    public async Task FetchConnectorSettingsAsync_Throws_WhenRowsRecordedOrgDoesNotMatchTheOrgTheCiphertextWasEncryptedFor()
    {
        var actualOrgId = Guid.NewGuid();
        var spoofedOrgId = Guid.NewGuid();
        var connectorName = typeof(FakeConnectorConfiguration).Assembly.GetName().Name!;

        // Ciphertext was protected for actualOrgId, but the row claims to belong to spoofedOrgId -
        // e.g. copied or relinked between education organizations. The bound purpose must reject it
        // rather than silently decrypting under the wrong tenant.
        var protector = ConnectorSettingsProtector.CreateBoundProtector(dataProtectionProvider, connectorName, actualOrgId);
        var row = new EducationOrganizationConnectorSettings
        {
            EducationOrganizationId = spoofedOrgId,
            Connector = connectorName,
            Settings = BuildSettingsDocument(protector, typeof(FakeConnectorConfiguration).FullName!, new { Username = "svc", Password = "p@ss" })
        };

        connectorSettingsRepo
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<ISpecification<EducationOrganizationConnectorSettings>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(row);

        await Assert.ThrowsAsync<CryptographicException>(
            () => resolver.FetchConnectorSettingsAsync<FakeConnectorConfiguration>(spoofedOrgId));
    }
}
