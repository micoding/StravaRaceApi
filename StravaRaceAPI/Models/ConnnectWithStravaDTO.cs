using System.Text.Json.Serialization;

namespace StravaRaceAPI.Models;

public class ConnnectWithStravaDTO
{
    [JsonPropertyName("athlete")] public AthleteDTO athlete { get; set; } = null!;

    public TokenDTO Token { get; set; } = null!;
}