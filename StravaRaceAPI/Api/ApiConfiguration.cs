namespace StravaRaceAPI.Api;

/// <summary>
///     Object representing API configuration, its Client ID and Client secret.
/// </summary>
public class ApiConfiguration
{
    public int ClientId { get; set; }
    public string? ClientSecret { get; set; }

    public string? WebhookVerifyToken { get; set; }

    /// <summary>
    ///     Static object containing API configuration.
    /// </summary>
    public static ApiConfiguration Current { get; set; } = null!;
}