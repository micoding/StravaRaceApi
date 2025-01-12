namespace StravaRaceAPI.Services;

/// <summary>
///     LoggedIn Users data from the Token.
/// </summary>
public interface IUserContextService
{
    /// <summary>
    ///     User inside Token.
    /// </summary>
    ClaimsPrincipal User { get; }

    /// <summary>
    ///     User ID inside the Token.
    /// </summary>
    int? GetUserId { get; }

    /// <summary>
    ///     Username inside the Token.
    /// </summary>
    string GetUserName { get; }
}

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User!;

    public int? GetUserId =>
        int.Parse(User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value!);

    public string GetUserName => User.FindFirst(c => c.Type == ClaimTypes.Name)!.Value;
}