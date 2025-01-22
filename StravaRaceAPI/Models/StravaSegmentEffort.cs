namespace StravaRaceAPI.Models;

public class StravaSegmentEffort
{
    [JsonPropertyName("elapsed_time")] public uint ElapsedTime { get; set; }
    [JsonPropertyName("start_date")] public DateTime StartDate { get; set; }
    [JsonPropertyName("segment")] public Segment Segment { get; set; }
}