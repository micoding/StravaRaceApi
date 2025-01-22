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
        var seg = await context.Segments.FirstOrDefaultAsync(x => x.Id == segmentId);
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

    /// <summary>
    ///     Add result to the Event.
    /// </summary>
    /// <param name="context">ApiDBContext</param>
    /// <param name="userId">ID of the user with the result.</param>
    /// <param name="eventId">ID of the Event with the result.</param>
    /// <param name="segmentId">ID of the segment with the result.</param>
    /// <param name="time">The result time in seconds.</param>
    public static async Task AddResult(this ApiDBContext context, int userId, ulong eventId, ulong segmentId, uint time)
    {
        var newResult = new Result();
        var usr = await context.TryGetUser(userId);
        var ev = await context.TryGetEvent(eventId);
        if (!ev.Competitors.Contains(usr)) throw new NotFoundException("User not enrolled to the event.");
        var segment = ev.Segments.FirstOrDefault(s => s.Id == segmentId);
        if (segment is null) throw new NotFoundException("Segment not present in the event.");

        newResult.Event = ev;
        newResult.Segment = segment;
        newResult.Time = time;
        newResult.User = usr;

        if (await context.Results.AnyAsync(r =>
                r.EventId == newResult.Event.Id && r.SegmentId == newResult.Segment.Id &&
                r.UserId == newResult.User.Id))
            throw new ItemExistsException("Result already exists");

        await context.Results.AddAsync(newResult);
        await context.SaveChangesAsync();
    }
}