using StravaRaceAPI.Entities;

namespace StravaRaceAPI.Models;

public class ShowEventDTO
{
    public ulong Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<AthleteDTO> Competitors { get; set; } = new();
    public List<Result> Results { get; set; } = new();
    public List<ShowEventDTO> Segments { get; set; } = new();
    public DateTime CreationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AthleteDTO Creator { get; set; } = null!;
    public int CreatorId { get; set; }
}