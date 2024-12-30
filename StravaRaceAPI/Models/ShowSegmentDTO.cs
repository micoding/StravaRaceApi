using StravaRaceAPI.Entities;

namespace StravaRaceAPI.Models;

public class ShowSegmentDTO : SegmentDTO
{
    public List<Result> Results { get; set; } = null!;
}