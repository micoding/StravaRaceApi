namespace StravaRaceAPI;

public class AuthenticationOptions
{
    public string JwtKey { get; set; } = null!;
    public string JwtIssuer { get; set; } = null!;
    public int JwtExpireDays { get; set; }
}