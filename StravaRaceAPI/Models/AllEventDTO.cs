namespace StravaRaceAPI.Models;

public class AllEventDTO
{
    public ulong Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}