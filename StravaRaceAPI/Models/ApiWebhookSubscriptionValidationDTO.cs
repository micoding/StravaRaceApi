namespace StravaRaceAPI.Models;

public class ApiWebhookSubscriptionValidationDTO
{
    [JsonPropertyName("hub.mode")] public string HubMode { get; set; } = null!;
    [JsonPropertyName("hub.challenge")] public string HubChallenge { get; set; } = null!;
    [JsonPropertyName("hub.verify_token")] public string HubVerifyToken { get; set; } = null!;
}