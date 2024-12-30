namespace StravaRaceAPI.Entities;

public class Token
{
    public User User { get; set; } = null!;
    public int UserId { get; set; }
    public string RefreshToken { get; set; } = null!;

    public string AccessToken { get; set; } = null!;

    public DateTime ExpirationOfToken { get; set; }
}