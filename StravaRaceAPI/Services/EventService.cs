namespace StravaRaceAPI.Services;

public interface IEventService
{
    /// <summary>
    ///     Create new Event in API.
    /// </summary>
    /// <param name="eventDto">DTO of the Event to be created.</param>
    /// <returns cref="Event">Created Event.</returns>
    Task<Event> CreateEvent(CreateEventDTO eventDto);

    /// <summary>
    ///     Get all Events.
    /// </summary>
    /// <returns cref="Event">List of all the Events.</returns>
    Task<List<Event>> GetAllEvents();

    /// <summary>
    ///     Get Event by id.
    /// </summary>
    /// <param name="eventId">ID of the Event to return.</param>
    /// <returns cref="Event">Event with the ID.</returns>
    Task<Event> GetEvent(ulong eventId);

    /// <summary>
    ///     Add competitors to the Event.
    /// </summary>
    /// <param name="eventId">ID of the event to be modified.</param>
    /// <param name="competitorIds">List of IDs of the competitors to be added.</param>
    Task AddCompetitors(ulong eventId, List<int> competitorIds);

    /// <summary>
    ///     Add segments to the Event.
    /// </summary>
    /// <param name="eventId">ID of the event to be modified.</param>
    /// <param name="segmentIds">List of IDs of the segments to be added.</param>
    Task AddSegments(ulong eventId, List<ulong> segmentIds);

    /// <summary>
    ///     Add result to the Event.
    /// </summary>
    /// <param name="userId">ID of the user with the result.</param>
    /// <param name="eventId">ID of the Event with the result.</param>
    /// <param name="segmentId">ID of the segment with the result.</param>
    /// <param name="time">The result time in seconds.</param>
    Task AddResult(int userId, ulong eventId, ulong segmentId, uint time);

    /// <summary>
    ///     Remove competitors from the Event.
    /// </summary>
    /// <param name="eventId">ID of the Event to be modified.</param>
    /// <param name="competitorIds">List of IDs of the competitors to be removed.</param>
    Task RemoveCompetitors(ulong eventId, List<int> competitorIds);

    /// <summary>
    ///     Remove segments from the Event.
    /// </summary>
    /// <param name="eventId">ID of the Event to be modified.</param>
    /// <param name="segmentsIds">List of IDs of the segments to be removed.</param>
    Task RemoveSegments(ulong eventId, List<ulong> segmentsIds);
}

public class EventService : IEventService
{
    private readonly ApiDBContext _context;
    private readonly IMapper _mapper;
    private readonly ISegmentClient _segmentClient;
    private readonly IUserContextService _userContextService;

    public EventService(ApiDBContext context, IMapper mapper, IUserContextService userContextService,
        ISegmentClient segmentClient)
    {
        _context = context;
        _mapper = mapper;
        _userContextService = userContextService;
        _segmentClient = segmentClient;
    }

    /// <inheritdoc />
    public async Task<Event> CreateEvent(CreateEventDTO eventDto)
    {
        var newEvent = _mapper.Map<Event>(eventDto);

        newEvent.Creator = await _context.TryGetUser(eventDto.CreatorId);

        newEvent.Segments.AddRange(await ResolveSegments(eventDto.SegmentIds));

        await _context.Events.AddAsync(newEvent);
        await _context.SaveChangesAsync();

        return newEvent;
    }

    /// <inheritdoc />
    public async Task<List<Event>> GetAllEvents()
    {
        var events = await _context.Events.ToListAsync();
        if (events.Count == 0)
            throw new NotFoundException("No events found");

        return events;
    }

    /// <inheritdoc />
    public async Task<Event> GetEvent(ulong eventId)
    {
        return await _context.TryGetEvent(eventId);
    }

    /// <inheritdoc />
    public async Task AddCompetitors(ulong eventId, List<int> competitorIds)
    {
        var comp = await _context.TryGetEvent(eventId);

        var notExist = competitorIds.Where(x => _context.Users.All(usr => usr.Id != x));

        if (notExist.Any())
            throw new NotFoundException("Given users not found");

        var notEnrolled = competitorIds.Where(x => comp.Competitors.All(c => c.Id != x)).ToList();

        if (notEnrolled.Count == 0)
            throw new CompetitorAssignedToEventException("All competitors already assigned to this event");

        var usersWithEvents = notEnrolled
            .Select(competitorId => new UserWithEvent { UserId = competitorId, EventId = eventId }).ToList();

        await _context.UsersWithEvents.AddRangeAsync(usersWithEvents);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task AddSegments(ulong eventId, List<ulong> segmentIds)
    {
        var comp = await _context.TryGetEvent(eventId);

        var segmentsToAssign = await ResolveSegments(segmentIds);

        var notEnrolled = segmentsToAssign.Where(x => comp.Segments.TrueForAll(s => s.Id != x.Id)).ToList();

        if (notEnrolled.Count == 0) throw new Exception("All segments already assigned to this event");

        comp.Segments.AddRange(notEnrolled);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task AddResult(int userId, ulong eventId, ulong segmentId, uint time)
    {
        var newResult = new Result();
        var usr = await _context.TryGetUser(userId);
        var ev = await _context.TryGetEvent(eventId);
        var seg = await ResolveSegments([segmentId]);

        newResult.Event = ev;
        newResult.Segment = seg.First();
        newResult.Time = time;
        newResult.User = usr;

        if (await _context.Results.AnyAsync(r =>
                r.EventId == newResult.EventId && r.SegmentId == newResult.SegmentId && r.UserId == newResult.UserId))
            throw new ItemExistsException("Result already exists");

        await _context.Results.AddAsync(newResult);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoveCompetitors(ulong eventId, List<int> competitorIds)
    {
        var comp = await _context.TryGetEvent(eventId);

        var notExist = competitorIds.Where(x => _context.Users.All(usr => usr.Id != x));

        if (notExist.Any())
            throw new NotFoundException("Given users not found");

        var toRemove = competitorIds.Where(x => comp.Competitors.All(c => c.Id == x)).ToList();

        if (toRemove.Count == 0)
            throw new NotFoundException("All competitors not assigned to this event");

        var competitorsToRemove = await _context.UsersWithEvents.Where(x => toRemove.Contains(x.UserId)).ToListAsync();

        _context.UsersWithEvents.RemoveRange(competitorsToRemove);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemoveSegments(ulong eventId, List<ulong> segmentsIds)
    {
        var comp = await _context.TryGetEvent(eventId);

        var toRemove = segmentsIds.Where(x => comp.Segments.All(c => c.Id == x)).ToList();

        if (toRemove.Count == 0)
            throw new NotFoundException("All segments not assigned to this event");

        var eventsToRemove = await _context.UsersWithEvents.Where(x => toRemove.Contains(x.EventId)).ToListAsync();

        _context.UsersWithEvents.RemoveRange(eventsToRemove);
        await _context.SaveChangesAsync();
    }

    private async Task<List<Segment>> ResolveSegments(List<ulong> segmentsId)
    {
        var segments = new List<Segment>();
        foreach (var id in segmentsId) segments.Add(await _context.GetSegment(id) ?? await TryPullSegment(id));
        return segments;
    }

    private async Task<Segment> TryPullSegment(ulong segmentId)
    {
        var segment = await _segmentClient.PullSegment(segmentId);

        await _context.Segments.AddAsync(segment);
        await _context.SaveChangesAsync();

        return segment;
    }
}