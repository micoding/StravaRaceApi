namespace StravaRaceAPI;

/// <summary>
///     Object representing API authentication options.
/// </summary>
public class AuthenticationOptions
{
    public string JwtKey { get; set; } = null!;
    public string JwtIssuer { get; set; } = null!;
    public int JwtExpireDays { get; set; }
}