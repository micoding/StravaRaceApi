using Microsoft.EntityFrameworkCore;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Models;
using StravaRaceAPI.Services;

namespace StravaRaceAPI.Api;

public interface ITokenHandler
{
    /// <summary>
    ///     Gets accessToken.
    /// </summary>
    /// <returns>JWT access token.</returns>
    string GetAccessToken();
}

public class TokenHandler : ITokenHandler
{
    private readonly ApiDBContext _context;
    private readonly Token _token;

    public TokenHandler(IUserContextService userService, ApiDBContext context)
    {
        _context = context;

        var user = _context.Users.Include(u => u.Token).SingleOrDefault(u => u.Id == userService.GetUserId);
        var token = user?.Token;
        _token = token ?? throw new TokenNotFoundException($"Token of the user: {userService.GetUserId} not found!");

        PrepareToken().Wait();
    }

    /// <inheritdoc />
    public string GetAccessToken()
    {
        Task.Run(PrepareToken().Wait);
        return _token.AccessToken;
    }

    private async Task PrepareToken()
    {
        if (string.IsNullOrEmpty(_token.AccessToken))
            GetNewToken();
        if (!IsValid())
        {
            await Refresh();
            _context.SaveChangesAsync().Wait();
        }
    }

    private bool IsValid()
    {
        return _token.ExpirationOfToken - TimeSpan.FromSeconds(10) > DateTimeOffset.UtcNow;
    }

    private async Task Refresh()
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

    private static void GetNewToken()
    {
        var tmpHttpClient = new HttpClient();
        tmpHttpClient.GetAsync(ApiConfiguration.Current.GetAuthorizationCode());
    }
}