namespace StravaRaceAPI.Models;

public class SegmentDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Distance { get; set; }
    public float Elevation { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
}