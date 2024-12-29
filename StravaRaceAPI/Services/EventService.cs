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
    Task<Event> GetEvent(int eventId);
    Task AddCompetitor(int eventId, int competitorId);
    Task AddCompetitors(int eventId, List<int> competitorIds);
    Task AddSegments(int eventId, List<int> segmentIds);
    Task AddResult(int userId, int eventId, int segmentId, uint time);
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

        newEvent.Segments.Add(await ResolveSegment(eventDto.SegmentIds.FirstOrDefault()));

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

    public async Task<Event> GetEvent(int eventId)
    {
        return await _context.TryGetEvent(eventId);
    }

    public async Task AddCompetitor(int eventId, int competitorId)
    {
        var comp = await _context.TryGetEvent(eventId);

        var usr = await _context.TryGetUser(competitorId);

        if (comp.Competitors.Any(x => x.Id == competitorId))
            throw new Exception($"Competitor with id {competitorId} already assigned to this event");

        comp.Competitors.Add(usr);
        await _context.SaveChangesAsync();
    }

    public async Task AddCompetitors(int eventId, List<int> competitorIds)
    {
        var comp = await _context.TryGetEvent(eventId);

        var notExist = competitorIds.Where(x => _context.Users.Any(usr => usr.Id != x));

        if (notExist.Any())
            throw new NotFoundException("Given users not found");

        var notEnrolled = comp.Competitors.Where(x => !competitorIds.Contains(x.Id)).ToList();

        if (notEnrolled.Any())
            throw new Exception("All competitors already assigned to this event");

        comp.Competitors.AddRange(notEnrolled);
        await _context.SaveChangesAsync();
    }

    public async Task AddSegments(int eventId, List<int> segmentIds)
    {
        var ev = await _context.TryGetEvent(eventId);

        var segmentsNonExist = segmentIds.Where(x => _context.Segments.All(seg => seg.Id != (ulong)x));
        if (segmentsNonExist.Any())
        {
        }

        var notEnrolled = ev.Segments.Where(x => segmentIds.TrueForAll(id => (ulong)id != x.Id)).ToList();

        if (notEnrolled.Any())
            throw new Exception("All competitors already assigned to this event");

        ev.Segments.AddRange(notEnrolled);
        await _context.SaveChangesAsync();
    }

    public async Task AddResult(int userId, int eventId, int segmentId, uint time)
    {
        var newResult = new Result();
        var usr = await _context.TryGetUser(userId);
        var ev = await _context.TryGetEvent(eventId);
        var seg = await ResolveSegment(segmentId);

        newResult.Event = ev;
        newResult.Segment = seg;
        newResult.Time = time;
        newResult.User = usr;

        if (await _context.Results.AnyAsync(r =>
                r.EventId == newResult.EventId && r.SegmentId == newResult.SegmentId && r.UserId == newResult.UserId))
            throw new ItemExists("Result already exists");

        await _context.Results.AddAsync(newResult);
        await _context.SaveChangesAsync();
    }

    private async Task<Segment> ResolveSegment(int segmentId)
    {
        var seg = await _context.GetSegment(segmentId) ?? await TryPullSegment(segmentId);
        return seg;
    }
    
    private async Task<Segment> TryPullSegment(int segmentId)
    {
        var segment = await _segmentClient.PullSegment(segmentId);

        await _context.Segments.AddAsync(segment);
        await _context.SaveChangesAsync();

        return segment;
    }
}