using System.Net.Http.Headers;

namespace StravaRaceAPI.Api.Clients;

public abstract class StravaApiClient
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger<StravaApiClient> Logger;
    protected readonly ITokenHandler TokenHandler;

    /// <summary>
    ///     Strava API Client base class.
    /// </summary>
    /// <param name="tokenHandler">User TokenHandler to be used for the Strava API requests.</param>
    /// <param name="logger">Logger</param>
    /// <param name="httpClient">HttpClient</param>
    protected StravaApiClient(ITokenHandler tokenHandler, ILogger<StravaApiClient> logger, IHttpClientFactory httpClient)
    {
        Logger = logger;
        TokenHandler = tokenHandler;
        HttpClient = httpClient.CreateClient("HttpClient");

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenHandler.GetAccessToken());
    }
}