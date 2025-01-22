namespace StravaRaceAPI.Models;

public class AthleteDTO
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("firstname")] public string FirstName { get; set; } = null!;

    [JsonPropertyName("lastname")] public string LastName { get; set; } = null!;

    [JsonPropertyName("username")] public string Username { get; set; } = null!;

    [JsonPropertyName("PhotoUrl")] public string PhotoUrl { get; set; } = null!;

    [JsonPropertyName("sex")] public string Sex { get; set; } = null!;
}