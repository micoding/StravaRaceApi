namespace StravaRaceAPI.Entities;

public class Result
{
    public ulong Id { get; set; }
    public ulong SegmentId { get; set; }
    public Segment Segment { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ulong EventId { get; set; }
    public Event Event { get; set; } = null!;

    public uint Time { get; set; }
}