using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;

namespace StravaRaceAPI.Services;

public interface IUserService
{
    Task<string> SignInWithStrava(User userToSignIn);
    Task<string> Refresh(User user);
    Task Delete(int userId);
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

    public async Task<string> Refresh(User user)
    {
        _context.ChangeTracker.Clear();
        _context.Users.Attach(user);
        _context.Tokens.Update(user.Token);
        await _context.SaveChangesAsync();

        return GenerateJwtToken(user);
    }

    public async Task Delete(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) throw new NotFoundException("User not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

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