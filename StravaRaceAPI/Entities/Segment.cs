namespace StravaRaceAPI.Entities;

public class Segment
{
    public ulong Id { get; set; }
    public string Name { get; set; } = null!;
    public float Distance { get; set; }
    public float Elevation { get; set; }
    public List<Event> Events { get; set; } = null!;
    public List<Result> Results { get; set; } = null!;
}