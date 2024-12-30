using System.Net.Http.Headers;

namespace StravaRaceAPI.Api.Clients;

public abstract class StravaApiClient
{
    protected readonly HttpClient HttpClient;
    
    /// <summary>
    /// Strava API Client base class.
    /// </summary>
    /// <param name="token">UserToken to be used to Strava API requests.</param>
    protected StravaApiClient(TokenHandler token)
    {
        HttpClient = new HttpClient();

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.GetAccessToken());
    }
}