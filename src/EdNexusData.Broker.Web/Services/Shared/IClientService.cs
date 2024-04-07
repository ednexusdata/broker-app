namespace src.Services.Shared;
public interface IClientService
{
   Task<IEnumerable<T>> GetApiResponseAsync<T>(
        HttpClient httpClient,
        string endpoint);
    Task<IEnumerable<T>> GetApiResponseAsync<T>(
        string endpoint,
        string bearerAccessToken);
}
