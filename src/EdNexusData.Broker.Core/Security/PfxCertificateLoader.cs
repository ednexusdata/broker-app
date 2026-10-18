using System.Security.Cryptography.X509Certificates;

namespace EdNexusData.Broker.Core.Security;

/// <summary>
/// Loads the PFX certificate(s) used to protect the ASP.NET Core Data Protection key ring. Shared
/// by EdNexusData.Broker.Web and EdNexusData.Broker.Worker so both hosts agree on how a missing
/// password is represented.
/// </summary>
public static class PfxCertificateLoader
{
    public static X509Certificate2 Load(string? path, string? password)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("DataProtection certificate path is not configured.");
        }

        // A blank/missing password and the literal string "null" both mean "this PFX has no password".
        return string.IsNullOrEmpty(password) || password == "null"
            ? X509CertificateLoader.LoadPkcs12FromFile(path, null)
            : X509CertificateLoader.LoadPkcs12FromFile(path, password);
    }
}
