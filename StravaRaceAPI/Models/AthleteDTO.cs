using System.Text.Json.Serialization;

namespace StravaRaceAPI.Models;

public class AthleteDTO
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("firstname")] public string FirstName { get; set; }

    [JsonPropertyName("lastname")] public string LastName { get; set; }

    [JsonPropertyName("username")] public string Userame { get; set; }

    public string PhotoUrl { get; set; }

    [JsonPropertyName("sex")] public string Sex { get; set; }
}