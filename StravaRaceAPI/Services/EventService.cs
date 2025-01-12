using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Models;

namespace StravaRaceAPI.Services;

public interface IEventService
{
    Task<Event> CreateEvent(CreateEventDTO eventDto);
    Task<List<Event>> GetAllEvents();
    Task<Event> GetEvent(ulong eventId);
    Task AddCompetitors(ulong eventId, List<int> competitorIds);
    Task AddSegments(ulong eventId, List<ulong> segmentIds);
    Task AddResult(int userId, ulong eventId, ulong segmentId, uint time);
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

    public async Task<Event> CreateEvent(CreateEventDTO eventDto)
    {
        var newEvent = _mapper.Map<Event>(eventDto);

        newEvent.Creator = await _context.TryGetUser(eventDto.CreatorId);

        newEvent.Segments.AddRange(await ResolveSegments(eventDto.SegmentIds));

        await _context.Events.AddAsync(newEvent);
        await _context.SaveChangesAsync();

        return newEvent;
    }

    public async Task<List<Event>> GetAllEvents()
    {
        var events = await _context.Events.ToListAsync();
        if (events.Count == 0)
            throw new NotFoundException("No events found");

        return events;
    }

    public async Task<Event> GetEvent(ulong eventId)
    {
        return await _context.TryGetEvent(eventId);
    }

    public async Task AddCompetitors(ulong eventId, List<int> competitorIds)
    {
        var comp = await _context.TryGetEvent(eventId);

        var notExist = competitorIds.Where(x => _context.Users.Any(usr => usr.Id != x));

        if (notExist.Any())
            throw new NotFoundException("Given users not found");

        var notEnrolled = competitorIds.Where(x => comp.Competitors.All(c => c.Id != x)).ToList();

        if (notEnrolled.Count == 0)
            throw new CompetitorAssignedToEventException("All competitors already assigned to this event");

        var usersWithEvents = notEnrolled.Select(competitorId => new UserWithEvent { UserId = competitorId, EventId = eventId }).ToList();
        
        await _context.UsersWithEvents.AddRangeAsync(usersWithEvents);
        await _context.SaveChangesAsync();
    }

    public async Task AddSegments(ulong eventId, List<ulong> segmentIds)
    {
        var comp = await _context.TryGetEvent(eventId);

        var segmentsToAssign = await ResolveSegments(segmentIds);

        var notEnrolled = segmentsToAssign.Where(x => comp.Segments.TrueForAll(s => s.Id != x.Id)).ToList();

        if (notEnrolled.Count == 0) throw new Exception("All segments already assigned to this event");

        comp.Segments.AddRange(notEnrolled);
        await _context.SaveChangesAsync();
    }

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

    private async Task<List<Segment>> ResolveSegments(List<ulong> segmentsId)
    {
        var segments = new List<Segment>();
        foreach (var id in segmentsId)
        {
            segments.Add(await _context.GetSegment(id) ?? await TryPullSegment(id));
        }
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