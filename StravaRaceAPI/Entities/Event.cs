namespace StravaRaceAPI.Entities;

public class Event
{
    public ulong Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<User> Competitors { get; set; } = new();
    public List<Result> Results { get; set; } = new();
    public List<Segment> Segments { get; set; } = new();
    public DateTime CreationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public User Creator { get; set; }
    public int CreatorId { get; set; }
}