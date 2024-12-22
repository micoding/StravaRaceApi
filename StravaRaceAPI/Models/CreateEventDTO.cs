namespace StravaRaceAPI.Models;

public class CreateEventDTO
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<int> SegmentIds { get; set; }
    public int CreatorId { get; set; }
}