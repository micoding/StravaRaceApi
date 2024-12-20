namespace StravaRaceAPI.Entities;

public class RaceSegment
{
    public Event Event { get; set; }
    public ulong EventId { get; set; }

    public Segment Segment { get; set; }
    public ulong SegmentId { get; set; }
}