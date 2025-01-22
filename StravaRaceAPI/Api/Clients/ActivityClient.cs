namespace StravaRaceAPI.Api.Clients;

public interface IActivityClient
{
    Task<StravaActivity> GetActivityByIdAsync(ulong activityId);
}

public class ActivityClient : StravaApiClient, IActivityClient
{
    public ActivityClient(ITokenHandlerContext tokenHandler, ILogger<StravaApiClient> logger,
        IHttpClientFactory httpClient) :
        base(tokenHandler, logger, httpClient)
    {
    }

    public async Task<StravaActivity> GetActivityByIdAsync(ulong activityId)
    {
        var response = await HttpClient.GetAsync(Endpoints.Activities + $"/{activityId}");
        if (!response.IsSuccessStatusCode)
            throw new ApiCommunicationError($"Communication Error: {response.ReasonPhrase}");

        var activity = await response.Content.ReadFromJsonAsync<StravaActivity>() ??
                       throw new NotFoundException($"Activity with id: {activityId} not found.");

        Logger.LogInformation(await response.Content.ReadAsStringAsync());

        return activity;
    }
}