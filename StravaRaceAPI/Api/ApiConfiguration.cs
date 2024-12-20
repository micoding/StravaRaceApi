namespace StravaRaceAPI.Api;

public class ApiConfiguration
{
    public int ClientId { get; set; }
    public string ClientSecret { get; set; }
    public static ApiConfiguration Current { get; set; }
}