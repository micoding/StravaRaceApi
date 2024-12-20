namespace StravaRaceAPI.Entities;

public class Token
{
    public User User { get; set; }
    public int UserId { get; set; }
    public string RefreshToken { get; set; }

    public string AccessToken { get; set; }

    public DateTime ExpirationOfToken { get; set; }
}