namespace StravaRaceAPI.Api.Clients;

public interface IAthleteClient
{
    /// <summary>
    ///     Asynchronously receives the currently authenticated athlete.
    /// </summary>
    /// <returns>The currently authenticated athlete.</returns>
    /// <exception cref="ApiCommunicationError">When could not get authenticated athlete.</exception>
    /// <exception cref="CanNotReadException">When AthleteDTO could not be read.</exception>
    Task<AthleteDTO> GetAthleteAsync();
}

public class TestAthleteClient : StravaApiClient, IAthleteClient
{
    public TestAthleteClient(ITokenHandlerContext tokenHandler, ILogger<StravaApiClient> logger,
        IHttpClientFactory httpClientFactory) : base(tokenHandler, logger, httpClientFactory)
    {
    }

    /// <inheritdoc />
    public async Task<AthleteDTO> GetAthleteAsync()
    {
        var response = await HttpClient.GetAsync(Endpoints.Athlete);
        if (!response.IsSuccessStatusCode) throw new ApiCommunicationError("Error calling get athlete.");

        var user = await response.Content.ReadFromJsonAsync<AthleteDTO>() ??
                   throw new CanNotReadException("Athlete DTO not found.");

        Logger.LogInformation(await response.Content.ReadAsStringAsync());

        return user;
    }
}