using System.Net.Http.Headers;

namespace StravaRaceAPI.Api.Clients;

public interface IActivityClientStandalone
{
    Task<StravaActivity> GetActivityById(ulong activityId);
}

public class ActivityClientStandalone : IActivityClientStandalone
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ActivityClientStandalone> _logger;
    private readonly ITokenHandler _tokenHandler;

    public ActivityClientStandalone(IHttpClientFactory httpClientFactory, ILogger<ActivityClientStandalone> logger,
        ITokenHandlerInject tokenHandler)
    {
        _logger = logger;
        _tokenHandler = tokenHandler;
        _httpClient = httpClientFactory.CreateClient("HttpClient");
    }

    public async Task<StravaActivity> GetActivityById(ulong activityId)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _tokenHandler.GetAccessToken());

        var response = await _httpClient.GetAsync(Endpoints.Activities + $"/{activityId}");
        if (!response.IsSuccessStatusCode)
            throw new ApiCommunicationError($"Communication Error: {response.ReasonPhrase}");

        var activity = await response.Content.ReadFromJsonAsync<StravaActivity>() ??
                       throw new NotFoundException($"Activity with id: {activityId} not found.");

        _logger.LogInformation(await response.Content.ReadAsStringAsync());

        return activity;
    }
}