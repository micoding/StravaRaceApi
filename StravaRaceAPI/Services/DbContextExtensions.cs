using Microsoft.EntityFrameworkCore;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;

namespace StravaRaceAPI.Services;

public static class DbContextExtensions
{
    /// <summary>
    ///     Try to obtain Event object including Competitons and Results.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="eventId">Event id to be obtained.</param>
    /// <returns>Event object not null.</returns>
    /// <exception cref="NotFoundException">When event not found.</exception>
    public static async Task<Event> TryGetEvent(this ApiDBContext context, int eventId)
    {
        var ev = await context.Events
            .Include(x => x.Competitors)
            .Include(x => x.Results)
            .FirstOrDefaultAsync(x => x.Id == (ulong)eventId);
        if (ev is null)
            throw new NotFoundException(ErrorMessages.EventNotFoundMessage(eventId));
        return ev;
    }

    public static async Task<User> TryGetUser(this ApiDBContext context, int userId)
    {
        var usr = await context.Users.Include(u => u.Token).FirstOrDefaultAsync(x => x.Id == userId);
        if (usr is null)
            throw new NotFoundException(ErrorMessages.UserNotFoundMessage(userId));
        return usr;
    }

    public static async Task<Segment?> GetSegment(this ApiDBContext context, int segmentId)
    {
        var seg = await context.Segments.FirstOrDefaultAsync(x => x.Id == (ulong)segmentId);
        return seg;
    }

    // public static async Task<List<Segment>> TryGetSegments(this ApiDBContext context, List<int> segmentId)
    // {
    //     var newSegments = new List<Segment>();
    //     foreach (var segmentId in segmentId)
    //     {
    //         var seg = await context.Segments.FirstOrDefaultAsync(x => x.Id == (ulong)segmentId);
    //         if (seg is null)
    //             throw new NotFoundException(ErrorMessages.EventNotFoundMessage(segmentId));
    //     }
    //
    //     var seg = await context.Segments.FirstOrDefaultAsync(x => x.Id == (ulong)segmentId);
    //     if (seg is null)
    //         throw new NotFoundException(ErrorMessages.EventNotFoundMessage(segmentId));
    //     return seg;
    // }


    public static async Task<bool> UsersExist(this ApiDBContext context, List<int> userIds)
    {
        return await Task.FromResult(userIds.All(id => context.Users.Any(u => u.Id == id)));
    }
}