using Microsoft.EntityFrameworkCore;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;

namespace StravaRaceAPI.Services;

public static class DbContextExtensions
{
    /// <summary>
    ///     Try to obtain Event object including Competitors and Results.
    /// </summary>
    /// <param name="context">ApiDBContext.</param>
    /// <param name="eventId">Event id to be obtained.</param>
    /// <returns>Event object not null.</returns>
    /// <exception cref="NotFoundException">When event not found.</exception>
    public static async Task<Event> TryGetEvent(this ApiDBContext context, ulong eventId)
    {
        var ev = await context.Events
            .Include(x => x.Competitors)
            .Include(x => x.Results)
            .Include(x => x.Segments)
            .FirstOrDefaultAsync(x => x.Id == eventId);
        if (ev is null)
            throw new NotFoundException(ErrorMessages.EventNotFoundMessage(eventId));
        return ev;
    }

    /// <summary>
    ///     Try to obtain User by id.
    /// </summary>
    /// <param name="context">ApiDBContext</param>
    /// <param name="userId">User id.</param>
    /// <returns cref="User">User object.</returns>
    /// <exception cref="NotFoundException">When user with the ID not found.</exception>
    public static async Task<User> TryGetUser(this ApiDBContext context, int userId)
    {
        var usr = await context.Users.Include(u => u.Token).FirstOrDefaultAsync(x => x.Id == userId);
        if (usr is null)
            throw new NotFoundException(ErrorMessages.UserNotFoundMessage(userId));
        return usr;
    }

    /// <summary>
    ///     Get segment by id.
    /// </summary>
    /// <param name="context">ApiDBContext</param>
    /// <param name="segmentId">Segment id.</param>
    /// <returns cref="Segment">Segment object.</returns>
    public static async Task<Segment?> GetSegment(this ApiDBContext context, ulong segmentId)
    {
        var seg = await context.Segments.FirstOrDefaultAsync(x => x.Id == (ulong)segmentId);
        return seg;
    }

    /// <summary>
    ///     Does user with the id exit?
    /// </summary>
    /// <param name="context">ApiDBContext</param>
    /// <param name="userIds">User to find id.</param>
    /// <returns>Bool.</returns>
    public static async Task<bool> UsersExist(this ApiDBContext context, List<int> userIds)
    {
        return await Task.FromResult(userIds.All(id => context.Users.Any(u => u.Id == id)));
    }
}