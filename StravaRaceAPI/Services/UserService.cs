using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;

namespace StravaRaceAPI.Services;

public interface IUserService
{
    Task<User> SignInWithStrava(User userToSignIn);
    Task<User> Refresh(User user);
    Task Delete(int userId);
}

public class UserService : IUserService
{
    private readonly ApiDBContext _context;
    private readonly IMapper _mapper;

    public UserService(ApiDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<User> SignInWithStrava(User userToSignIn)
    {
        var user = await _context.Users
            .Include(u => u.Token)
            .FirstOrDefaultAsync(u => u.Id == userToSignIn.Id);

        if (user is not null) return await Refresh(userToSignIn);

        _context.Users.Add(userToSignIn);
        await _context.SaveChangesAsync();

        return userToSignIn;
    }

    public async Task<User> Refresh(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task Delete(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) throw new NotFoundException("User not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}