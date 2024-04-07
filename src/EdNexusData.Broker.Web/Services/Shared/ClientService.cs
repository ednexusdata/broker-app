using System.Net.Http.Headers;

namespace src.Services.Shared;
public class ClientService : IClientService
{
    public readonly IHttpClientFactory _httpClientFactory;

    public ClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task<IEnumerable<T>> GetApiResponseAsync<T>(
        HttpClient httpClient,
        string endpoint)
    {
        try
        {
            var response = await httpClient.GetAsync($"{endpoint}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<T>>();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetApiResponseAsync<T>(
        string endpoint,
        string bearerAccessToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerAccessToken);
            var uri = new Uri(endpoint);
            httpClient.BaseAddress = new Uri(uri.GetLeftPart(UriPartial.Authority));
            var response = await httpClient.GetAsync($"{endpoint}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<T>>();
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
