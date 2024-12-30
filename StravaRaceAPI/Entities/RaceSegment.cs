namespace StravaRaceAPI.Entities;

public class RaceSegment
{
    public Event Event { get; set; } = null!;
    public ulong EventId { get; set; }

    public Segment Segment { get; set; } = null!;
    public ulong SegmentId { get; set; }
}