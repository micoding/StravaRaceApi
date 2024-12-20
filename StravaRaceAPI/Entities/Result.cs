namespace StravaRaceAPI.Entities;

public class Result
{
    public ulong Id { get; set; }
    public ulong SegmentId { get; set; }
    public Segment Segment { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public ulong EventId { get; set; }
    public Event Event { get; set; }

    public uint Time { get; set; }
}