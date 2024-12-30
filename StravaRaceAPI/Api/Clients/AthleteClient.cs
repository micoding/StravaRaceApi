using StravaRaceAPI.Models;

namespace StravaRaceAPI.Api.Clients;

public class AthleteClient : StravaApiClient
{
    public AthleteClient(TokenHandler token) : base(token)
    {
    }

    /// <summary>
    ///     Asynchronously receives the currently authenticated athlete.
    /// </summary>
    /// <returns>The currently authenticated athlete.</returns>
    public async Task<AthleteDTO> GetAthleteAsync()
    {
        AthleteDTO user = null;
        var response = await HttpClient.GetAsync(Endpoints.Athlete);
        if (response.IsSuccessStatusCode)
        {
            user = await response.Content.ReadFromJsonAsync<AthleteDTO>();
            var str = await response.Content.ReadAsStringAsync();
        }

        return user;
    }
}