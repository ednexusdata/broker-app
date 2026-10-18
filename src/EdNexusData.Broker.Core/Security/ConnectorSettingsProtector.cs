using Microsoft.AspNetCore.DataProtection;

namespace EdNexusData.Broker.Core.Security;

/// <summary>
/// Centralizes the IDataProtector purpose chain used to encrypt connector credentials, so
/// EducationOrganizationConnectorSettings.Settings ciphertext is cryptographically bound to the
/// connector type and education organization it belongs to and cannot be decrypted if copied to
/// another record.
/// </summary>
public static class ConnectorSettingsProtector
{
    private const string BasePurpose = "SecureConnectionString";

    /// <summary>
    /// Protector used for both encrypting new settings and decrypting settings that were encrypted
    /// after per-record purpose binding was introduced.
    /// </summary>
    public static IDataProtector CreateBoundProtector(IDataProtectionProvider provider, string connectorName, Guid? educationOrganizationId)
        => provider.CreateProtector(BasePurpose, connectorName, educationOrganizationId?.ToString() ?? "none");

    /// <summary>
    /// Protector matching the pre-binding purpose string. Kept only so settings encrypted before
    /// this change can still be decrypted; new writes always use <see cref="CreateBoundProtector"/>.
    /// </summary>
    public static IDataProtector CreateLegacyProtector(IDataProtectionProvider provider)
        => provider.CreateProtector(BasePurpose);
}
