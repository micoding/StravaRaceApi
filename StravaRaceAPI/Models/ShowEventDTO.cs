using StravaRaceAPI.Entities;

namespace StravaRaceAPI.Models;

public class ShowEventDTO
{
    public ulong Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<AthleteDTO> Competitors { get; set; } = new();
    public List<Result> Results { get; set; } = new();
    public List<Segment> Segments { get; set; } = new();
    public DateTime CreationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AthleteDTO Creator { get; set; }
    public int CreatorId { get; set; }
}