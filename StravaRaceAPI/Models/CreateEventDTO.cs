namespace StravaRaceAPI.Models;

public class CreateEventDTO
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<int> SegmentIds { get; set; } = null!;
    public int CreatorId { get; set; }
}