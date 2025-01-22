namespace StravaRaceAPI.Models;

public class EventDataDTO
{
    [JsonPropertyName("object_type")] public string ObjectType { get; set; } = null!;
    [JsonPropertyName("object_id")] public ulong ObjectId { get; set; }
    [JsonPropertyName("aspect_type")] public string AspectType { get; set; } = null!;
    [JsonPropertyName("owner_id")] public int AthleteId { get; set; }
    [JsonPropertyName("subscription_id")] public int SubscriptionId { get; set; }
    [JsonPropertyName("event_time")] public long EventTime { get; set; }

    [JsonPropertyName("updates")] public Dictionary<string, string> Updates { get; set; } = new();
}