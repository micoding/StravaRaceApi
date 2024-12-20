namespace StravaRaceAPI.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime Birthday { get; set; }
    public Sex Gender { get; set; }

    public List<Event> Events { get; set; }
    public List<Event> CreatedEvents { get; set; }
    public List<Result> Results { get; set; }

    public Token Token { get; set; }
}