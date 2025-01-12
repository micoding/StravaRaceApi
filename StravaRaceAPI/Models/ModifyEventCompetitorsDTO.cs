using StravaRaceAPI.Entities;

namespace StravaRaceAPI.Models;

public class ModifyEventCompetitorsDTO
{
    public List<int> Competitors { get; set; } = new();
}