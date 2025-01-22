namespace StravaRaceAPI.Models;

public class StravaActivity
{
    [JsonPropertyName("id")] public ulong Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; }
    [JsonPropertyName("start_date")] public DateTime StartDate { get; set; }
    [JsonPropertyName("private")] public bool Private { get; set; }
    [JsonPropertyName("manual")] public bool Manual { get; set; }
    [JsonPropertyName("segments_efforts")] public List<StravaSegmentEffort> SegmentsEfforts { get; set; }
}