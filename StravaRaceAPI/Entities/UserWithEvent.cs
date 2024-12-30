namespace StravaRaceAPI.Entities;

public class UserWithEvent
{
    public Event Event { get; set; } = null!;
    public ulong EventId { get; set; }
    public User User { get; set; } = null!;
    public int UserId { get; set; }
}