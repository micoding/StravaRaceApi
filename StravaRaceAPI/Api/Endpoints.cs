namespace StravaRaceAPI.Api;

/// <summary>
///     Strava API Endpoints.
/// </summary>
public static class Endpoints
{
    public const string ApiMainToken = "https://api.strava.com/v3";
    public const string StravaMain = "https://www.strava.com";
    public const string ApiMain = StravaMain + "/api/v3";

    public const string Activities = ApiMain + "/activities";

    public const string Athlete = $"{ApiMain}/athlete";

    public const string Segment = $"{ApiMain}/segments";
    public const string SegmentsStarred = $"{Segment}/starred";

    private const string RefreshAccessToken = StravaMain + "/oauth/token";
    public const string AuthorizeRedirectCode = ApiMainToken + "/oauth/authorize";
    public const string AuthorizeCode = StravaMain + "/oauth/authorize";

    private const string RedirectUrl = "http://localhost";

    public const string WebHookUrl = ApiMain + "/push_subscriptions";

    public static string GetAuthorizationCode(this ApiConfiguration config, string redirectUrl = RedirectUrl)
    {
        return
            $"{AuthorizeCode}?client_id={config.ClientId}&redirect_uri={redirectUrl}&response_type=code&scope=activity:read_all";
    }

    public static string GetRefreshAccessToken(this ApiConfiguration config, Token token)
    {
        return
            $"{RefreshAccessToken}?client_id={config.ClientId}&client_secret={config.ClientSecret}&refresh_token={token.RefreshToken}&grant_type=refresh_token";
    }

    public static string GetWebhookSubscribe(this ApiConfiguration config, string redirectUrl = RedirectUrl)
    {
        return
            $"{RedirectUrl}?client_id={config.ClientId}&client_secret={config.ClientSecret}&callback_url={redirectUrl}&verify_token=webhooktoken";
    }
}