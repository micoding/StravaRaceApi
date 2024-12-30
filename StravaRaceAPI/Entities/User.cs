namespace StravaRaceAPI.Entities;

public class User : IIdentifier
{
    public string Username { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime Birthday { get; set; }
    public Sex Gender { get; set; }
    public string ProfilePictureUrl { get; set; } = null!;

    public List<Event> Events { get; set; } = null!;
    public List<Event> CreatedEvents { get; set; } = null!;
    public List<Result> Results { get; set; } = null!;

    public Token Token { get; set; } = null!;
    public int Id { get; set; }
}