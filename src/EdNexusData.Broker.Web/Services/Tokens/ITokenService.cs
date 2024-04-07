namespace src.Services.Tokens;
public interface ITokenService
{
  Task<string> GetAccessTokenAsync();
}
