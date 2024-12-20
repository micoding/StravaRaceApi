using StravaRaceAPI.Entities;
using StravaRaceAPI.Models;

namespace StravaRaceAPI.Api;

public class TokenHandler
{
    private readonly ApiDBContext _context;
    private readonly Token _token;

    public TokenHandler(Token token, ApiDBContext context)
    {
        _token = token;
        _context = context;
        PrepareToken().Wait();
    }

    public string GetAccessToken()
    {
        Task.Run(PrepareToken().Wait);
        return _token.AccessToken;
    }

    public async Task PrepareToken()
    {
        if (string.IsNullOrEmpty(_token.AccessToken))
            GetNewToken();
        if (!IsValid())
        {
            await Refresh();
            _context.SaveChangesAsync().Wait();
        }
    }

    public bool IsValid()
    {
        return _token.ExpirationOfToken - TimeSpan.FromSeconds(10) > DateTimeOffset.UtcNow;
    }

    public async Task Refresh()
    {
        var tmpHttpClient = new HttpClient();
        tmpHttpClient.BaseAddress = new Uri(ApiConfiguration.Current.GetRefreshAccessToken(_token));
        var response = tmpHttpClient.PostAsync(tmpHttpClient.BaseAddress, null).Result;

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to refresh token: {response.StatusCode}");

        var tokenDto = await response.Content.ReadFromJsonAsync<TokenDTO>();
        if (tokenDto is null)
            throw new HttpRequestException($"Failed to refresh token: {response.StatusCode}");

        _token.RefreshToken = tokenDto.RefreshToken;
        _token.AccessToken = tokenDto.AccessToken;
        _token.ExpirationOfToken = DateTime.UnixEpoch.AddSeconds(tokenDto.ExpiresAt);
    }

    public void GetNewToken()
    {
        var tmpHttpClient = new HttpClient();
        tmpHttpClient.GetAsync(ApiConfiguration.Current.GetAuthorizationCode());
    }
}