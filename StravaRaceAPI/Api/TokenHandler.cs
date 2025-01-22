namespace StravaRaceAPI.Api;

public interface ITokenHandler
{
    /// <summary>
    ///     Gets accessToken.
    /// </summary>
    /// <returns>JWT access token.</returns>
    public string GetAccessToken();
}

public abstract class TokenHandler : ITokenHandler
{
    private readonly ApiDBContext _context;

    protected TokenHandler(ApiDBContext context)
    {
        _context = context;
    }

    public Token Token { protected get; set; }

    /// <inheritdoc />
    public string GetAccessToken()
    {
        Task.Run(PrepareToken().Wait);
        return Token.AccessToken;
    }

    private async Task PrepareToken()
    {
        if (string.IsNullOrEmpty(Token.AccessToken))
            GetNewToken();
        if (!IsValid())
        {
            await Refresh();
            _context.SaveChangesAsync().Wait();
        }
    }

    private bool IsValid()
    {
        return Token.ExpirationOfToken - TimeSpan.FromSeconds(10) > DateTimeOffset.UtcNow;
    }

    private async Task Refresh()
    {
        var tmpHttpClient = new HttpClient();
        tmpHttpClient.BaseAddress = new Uri(ApiConfiguration.Current.GetRefreshAccessToken(Token));
        var response = tmpHttpClient.PostAsync(tmpHttpClient.BaseAddress, null).Result;

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to refresh token: {response.StatusCode}");

        var tokenDto = await response.Content.ReadFromJsonAsync<TokenDTO>();
        if (tokenDto is null)
            throw new HttpRequestException($"Failed to refresh token: {response.StatusCode}");

        Token.RefreshToken = tokenDto.RefreshToken;
        Token.AccessToken = tokenDto.AccessToken;
        Token.ExpirationOfToken = DateTime.UnixEpoch.AddSeconds(tokenDto.ExpiresAt);
    }

    private static void GetNewToken()
    {
        var tmpHttpClient = new HttpClient();
        tmpHttpClient.GetAsync(ApiConfiguration.Current.GetAuthorizationCode());
    }
}