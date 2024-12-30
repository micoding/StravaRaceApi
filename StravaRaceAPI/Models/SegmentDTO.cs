namespace StravaRaceAPI.Models;

public class SegmentDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public float Distance { get; set; }
    public float Elevation { get; set; }
    public string Country { get; set; } = null!;
    public string City { get; set; } = null!;
}