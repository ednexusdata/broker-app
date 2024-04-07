using System.Text.Json.Serialization;

namespace src.Models.Tokens;

public record TokenRequest(
    [property: JsonPropertyName("client_id")] string ClientId,
    [property: JsonPropertyName("client_secret")] string ClientSecret,
    [property: JsonPropertyName("grant_type")] string GrantType = "client_credentials");

