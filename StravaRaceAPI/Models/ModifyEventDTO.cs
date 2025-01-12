using StravaRaceAPI.Entities;

namespace StravaRaceAPI.Models;

public class ModifyEventDTO
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<User> Competitors { get; set; } = new();
    public List<Segment> Segments { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}