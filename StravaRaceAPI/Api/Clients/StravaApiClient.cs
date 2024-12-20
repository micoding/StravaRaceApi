using System.Net.Http.Headers;

namespace StravaRaceAPI.Api.Clients;

public class StravaApiClient
{
    protected readonly HttpClient _httpClient;

    public StravaApiClient(TokenHandler token)
    {
        _httpClient = new HttpClient();

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.GetAccessToken());
    }
}