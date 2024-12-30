using System.Net.Http.Headers;

namespace StravaRaceAPI.Api.Clients;

public abstract class StravaApiClient
{
    protected readonly HttpClient HttpClient;
    protected readonly ITokenHandler TokenHandler;
    protected readonly ILogger<StravaApiClient> Logger;
    
    /// <summary>
    /// Strava API Client base class.
    /// </summary>
    /// <param name="tokenHandler">User TokenHandler to be used for the Strava API requests.</param>
    /// <param name="logger">Logger</param>
    protected StravaApiClient(ITokenHandler tokenHandler, ILogger<StravaApiClient> logger)
    {
        Logger = logger;
        TokenHandler = tokenHandler;
        HttpClient = new HttpClient();

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenHandler.GetAccessToken());
    }
}