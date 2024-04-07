using System.Net.Http.Headers;
using System.Net.Mime;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using src.Models.Tokens;

namespace src.Services.Tokens;
public class TokenService : ITokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepository<EducationOrganizationConnectorSettings> _educationOrganizationConnectorSettings;

    public TokenService(
        IHttpClientFactory httpClientFactory,
        IRepository<EducationOrganizationConnectorSettings> educationOrganizationConnectorSettings)
    {
        _httpClientFactory = httpClientFactory;
        _educationOrganizationConnectorSettings = educationOrganizationConnectorSettings;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        try
        {
            var educationOrganizationSettings = await _educationOrganizationConnectorSettings.ListAsync();
            var educationOrganizationSetting = educationOrganizationSettings.FirstOrDefault(
                educationOrganization => educationOrganization.Connector == "EdNexusData.Broker.Connector.EdFiAlliance.EdFi"
            ) ?? throw new Exception("Education organization settings not found.");

            var connection = educationOrganizationSetting.Settings?.RootElement
                .GetProperty("EdNexusData.Broker.Connector.EdFiAlliance.EdFi.Configuration.Connection");

            var apiUrl = connection?.GetProperty("EdFiApiUrl").GetString();
            var clientId = connection?.GetProperty("Key").GetString();
            var clientSecret = connection?.GetProperty("Secret").GetString();

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                throw new Exception("Invalid connection details.");

            var tokenRequest = new TokenRequest(clientId, clientSecret);

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(apiUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            var response = await httpClient.PostAsync("/oauth/token", JsonContent.Create(tokenRequest));

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to obtain access token. StatusCode: {(int)response.StatusCode}");

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
            var accessToken = (tokenResponse?.AccessToken)
                              ?? throw new Exception("Failed to obtain access token. Response did not contain access_token.");
            return accessToken;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to obtain access token.", ex);
        }
    }
}
