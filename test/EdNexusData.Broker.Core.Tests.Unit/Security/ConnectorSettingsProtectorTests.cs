using System.Security.Cryptography;
using EdNexusData.Broker.Core.Security;
using Microsoft.AspNetCore.DataProtection;

namespace EdNexusData.Broker.Core.Security.Tests.Unit;

public class ConnectorSettingsProtectorTests
{
    private readonly IDataProtectionProvider provider = new EphemeralDataProtectionProvider();

    [Fact]
    public void BoundProtector_RoundTripsItsOwnCiphertext()
    {
        var orgId = Guid.NewGuid();
        var protector = ConnectorSettingsProtector.CreateBoundProtector(provider, "ConnectorA", orgId);

        var ciphertext = protector.Protect("super-secret-password");

        Assert.Equal("super-secret-password", protector.Unprotect(ciphertext));
    }

    [Fact]
    public void BoundProtector_CannotDecrypt_CiphertextFromDifferentConnector()
    {
        var orgId = Guid.NewGuid();
        var ciphertext = ConnectorSettingsProtector.CreateBoundProtector(provider, "ConnectorA", orgId).Protect("secret");

        var otherConnectorProtector = ConnectorSettingsProtector.CreateBoundProtector(provider, "ConnectorB", orgId);

        Assert.Throws<CryptographicException>(() => otherConnectorProtector.Unprotect(ciphertext));
    }

    [Fact]
    public void BoundProtector_CannotDecrypt_CiphertextFromDifferentEducationOrganization()
    {
        var connectorName = "ConnectorA";
        var ciphertext = ConnectorSettingsProtector.CreateBoundProtector(provider, connectorName, Guid.NewGuid()).Protect("secret");

        var otherOrgProtector = ConnectorSettingsProtector.CreateBoundProtector(provider, connectorName, Guid.NewGuid());

        Assert.Throws<CryptographicException>(() => otherOrgProtector.Unprotect(ciphertext));
    }

    [Fact]
    public void BoundProtector_WithNullEducationOrganizationId_RoundTripsButDoesNotCollideWithARealOrgId()
    {
        var connectorName = "ConnectorA";
        var noOrgProtector = ConnectorSettingsProtector.CreateBoundProtector(provider, connectorName, educationOrganizationId: null);

        var ciphertext = noOrgProtector.Protect("secret");
        Assert.Equal("secret", noOrgProtector.Unprotect(ciphertext));

        var realOrgProtector = ConnectorSettingsProtector.CreateBoundProtector(provider, connectorName, Guid.NewGuid());
        Assert.Throws<CryptographicException>(() => realOrgProtector.Unprotect(ciphertext));
    }

    [Fact]
    public void LegacyProtector_CannotDecrypt_CiphertextFromBoundProtector()
    {
        var boundCiphertext = ConnectorSettingsProtector.CreateBoundProtector(provider, "ConnectorA", Guid.NewGuid()).Protect("secret");

        var legacyProtector = ConnectorSettingsProtector.CreateLegacyProtector(provider);

        Assert.Throws<CryptographicException>(() => legacyProtector.Unprotect(boundCiphertext));
    }

    [Fact]
    public void BoundProtector_CannotDecrypt_CiphertextFromLegacyProtector()
    {
        var legacyCiphertext = ConnectorSettingsProtector.CreateLegacyProtector(provider).Protect("secret");

        var boundProtector = ConnectorSettingsProtector.CreateBoundProtector(provider, "ConnectorA", Guid.NewGuid());

        Assert.Throws<CryptographicException>(() => boundProtector.Unprotect(legacyCiphertext));
    }

    [Fact]
    public void LegacyProtector_RoundTripsItsOwnCiphertext()
    {
        var protector = ConnectorSettingsProtector.CreateLegacyProtector(provider);

        var ciphertext = protector.Protect("secret");

        Assert.Equal("secret", protector.Unprotect(ciphertext));
    }
}
