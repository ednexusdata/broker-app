using System.Text.Json.Serialization;

namespace src.Models.Tokens;

public record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
