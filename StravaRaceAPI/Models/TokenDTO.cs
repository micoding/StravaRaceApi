using System.Text.Json.Serialization;

namespace StravaRaceAPI.Models;

public class TokenDTO
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;

    [JsonPropertyName("expires_at")] public int ExpiresAt { get; set; }

    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = null!;
}