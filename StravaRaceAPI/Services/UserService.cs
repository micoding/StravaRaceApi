using System.IdentityModel.Tokens.Jwt;

namespace StravaRaceAPI.Services;

public interface IUserService
{
    /// <summary>
    ///     Sign in with the Strava account.
    /// </summary>
    /// <param name="userToSignIn" cref="User">User to be signed in.</param>
    /// <returns>JWT API token.</returns>
    Task<string> SignInWithStrava(User userToSignIn);

    /// <summary>
    ///     Refresh user and gets token form the Strava account.
    /// </summary>
    /// <param name="user" cref="User">User to be refreshed.</param>
    /// <returns>JWT API token.</returns>
    Task<string> Refresh(User user);

    /// <summary>
    ///     Delete user.
    /// </summary>
    /// <param name="userId">Users ID to be deleted.</param>
    Task Delete(int userId);

    /// <summary>
    ///     Generate JWT token.
    /// </summary>
    /// <param name="user" cref="User">User for which to generate token.</param>
    /// <returns>JWT API token.</returns>
    string GenerateJwtToken(User user);
}

public class UserService : IUserService
{
    private readonly AuthenticationOptions _authOptions;
    private readonly ApiDBContext _context;

    public UserService(ApiDBContext context, AuthenticationOptions authOptions)
    {
        _context = context;
        _authOptions = authOptions;
    }

    /// <inheritdoc />
    public async Task<string> SignInWithStrava(User userToSignIn)
    {
        var user = await _context.Users
            .Include(u => u.Token)
            .FirstOrDefaultAsync(u => u.Id == userToSignIn.Id);

        if (user is not null) return await Refresh(userToSignIn);

        _context.Users.Add(userToSignIn);
        await _context.SaveChangesAsync();

        return GenerateJwtToken(userToSignIn);
    }

    /// <inheritdoc />
    public async Task<string> Refresh(User user)
    {
        _context.ChangeTracker.Clear();
        _context.Users.Attach(user);
        _context.Tokens.Update(user.Token);
        await _context.SaveChangesAsync();

        return GenerateJwtToken(user);
    }

    /// <inheritdoc />
    public async Task Delete(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) throw new NotFoundException("User not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FirstName)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now + TimeSpan.FromDays(_authOptions.JwtExpireDays);

        var token = new JwtSecurityToken(_authOptions.JwtIssuer,
            _authOptions.JwtIssuer,
            claims,
            expires: expires,
            signingCredentials: creds);

        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }
}