namespace StravaRaceAPI.Entities;

public class UserWithEvent
{
    public Event Event { get; set; }
    public ulong EventId { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}