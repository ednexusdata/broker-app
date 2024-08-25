using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

public static class BrokerIdentityUser
{
    public static string GenerateRandomPassword(PasswordOptions? opts = null)
    {
        if (opts == null)
        {
            opts = new PasswordOptions
            {
                RequiredLength = 16,
                RequiredUniqueChars = 20,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };
        }

        // Create a random byte array
        byte[] randomBytes = new byte[opts.RequiredLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert the byte array to a base64-encoded string
        return Convert.ToBase64String(randomBytes);
    }
}

