using StravaRaceAPI.Entities;

namespace StravaRaceAPI.Models;

public class ShowSegmentDTO : SegmentDTO
{
    public List<ResultDTO> Results { get; set; } = null!;
}