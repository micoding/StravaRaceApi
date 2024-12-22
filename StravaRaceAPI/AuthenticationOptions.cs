namespace StravaRaceAPI;

public class AuthenticationOptions
{
    public string JwtKey { get; set; }
    public string JwtIssuer { get; set; }
    public int JwtExpireDays { get; set; }
}