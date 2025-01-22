using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using StravaRaceAPI;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Services;

namespace StravaRaceAPITests.Services;

[TestFixture]
public class TestEventService
{
    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<ApiDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var mappingConfig = new MapperConfiguration(config => { config.AddProfile(new StravaMappingProfile()); });
        _mapper = mappingConfig.CreateMapper();

        _userContextService = new Mock<IUserContextService>();
        _userContextService.Setup(s => s.GetUserId).Returns(1);

        _segmentClient = new Mock<ISegmentClient>();
        _segmentClient.Setup(r => r.PullSegment(It.IsAny<ulong>()))
            .Returns((ulong val) => Task.FromResult(PreapreObject.CreateSegment(val)));

        _context = new ApiDBContext(_options);
        _serviceUnderTest = new EventService(_context, _mapper, _userContextService.Object, _segmentClient.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private IMapper _mapper;
    private Mock<IUserContextService> _userContextService;
    private Mock<ISegmentClient> _segmentClient;
    private DbContextOptions<ApiDBContext> _options;
    private EventService _serviceUnderTest;
    private ApiDBContext _context;


    [Test]
    public void CreateEvent_CreatorIdNotExist_Throws404NotFoundException()
    {
        Assert.ThrowsAsync<NotFoundException>(() => _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO));
    }

    [Test]
    public void GetAllEvents_EventsEmpty_ReturnsNotFound()
    {
        var ex = Assert.Throws<AggregateException>(() => _serviceUnderTest.GetAllEvents().Wait());
        Assert.That(ex.InnerException, Is.TypeOf<NotFoundException>());
    }

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(11)]
    public async Task GetAllEvents_ReturnsAllEvents(int numberOfEvents)
    {
        await AddUser();

        var listOfEvents = new List<Event>();
        for (var i = 0; i < numberOfEvents; i++)
            listOfEvents.Add(await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO));

        var events = await _serviceUnderTest.GetAllEvents();
        Assert.That(listOfEvents, Is.EqualTo(events));
    }

    private async Task AddUser(int id = 1)
    {
        var user = PreapreObject.CreateUser(id);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    [Test]
    public async Task CreateEvent_WhenSegmentNotInDB_InvokesPullMethod()
    {
        await AddUser();
        _segmentClient.Setup(x => x.PullSegment(It.IsAny<ulong>()))
            .Returns((ulong val) => Task.FromResult(PreapreObject.CreateSegment(val))).Verifiable();

        await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        _segmentClient.Verify(x => x.PullSegment(It.IsAny<ulong>()), Times.Once);
    }

    [Test]
    public async Task CreateEvent_WhenSegmentInDB_DoesNotInvokePullMethod()
    {
        await AddUser();
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        await _context.SaveChangesAsync();

        _segmentClient.Setup(x => x.PullSegment(It.IsAny<ulong>()))
            .Returns((ulong val) => Task.FromResult(PreapreObject.CreateSegment(val))).Verifiable();

        await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        _segmentClient.Verify(x => x.PullSegment(It.IsAny<ulong>()), Times.Never);
    }

    [TestCase(1u)]
    [TestCase(0u)]
    [TestCase(10u)]
    public void GetEvent_EventNotExist_Throws404NotFoundException(ulong eventId)
    {
        var ex = Assert.Throws<AggregateException>(() => _serviceUnderTest.GetEvent(eventId).Wait());
        Assert.That(ex.InnerException, Is.TypeOf<NotFoundException>());
    }

    [Test]
    public async Task GetEvent_EventExist_ReturnsEvent()
    {
        await AddUser();

        var created = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        var createdEvent = _mapper.Map<Event>(created);

        var eventReturned = await _serviceUnderTest.GetEvent(createdEvent.Id);
        Assert.That(eventReturned, Is.EqualTo(createdEvent));
    }

    [Test]
    public async Task AddCompetitor_UserNotExist_Throw404NotFoundException()
    {
        await AddUser();
        await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        const int eventId = 1;
        const int competitorId = 2;

        var ex = Assert.Throws<AggregateException>(() =>
            _serviceUnderTest.AddCompetitors(eventId, [competitorId]).Wait());
        Assert.That(ex.InnerException, Is.TypeOf<NotFoundException>());
    }

    [Test]
    public async Task AddCompetitor_EventNotExist_Throw404NotFoundException()
    {
        await AddUser();
        const int eventId = 1;
        const int competitorId = 1;

        var ex = Assert.Throws<AggregateException>(() =>
            _serviceUnderTest.AddCompetitors(eventId, [competitorId]).Wait());
        Assert.That(ex.InnerException, Is.TypeOf<NotFoundException>());
    }

    [Test]
    public async Task AddCompetitor_AddsUserToEvent()
    {
        await AddUser();
        await AddUser(2);
        await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        const int eventId = 1;
        const int competitorId = 2;

        await _serviceUnderTest.AddCompetitors(eventId, [competitorId]);

        var returnedEvent = await _serviceUnderTest.GetEvent(eventId);
        Assert.That(returnedEvent.Competitors.Exists(x => x.Id == competitorId));
    }

    [Test]
    public async Task AddCompetitor_UserNotExist_ThrowItemExistsException()
    {
        await AddUser();
        await AddUser(2);
        const int eventId = 1;
        const int competitorId = 2;
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        createdEvent.Competitors.Add(_context.Users.First(x => x.Id == competitorId));
        await _context.SaveChangesAsync();

        var ex = Assert.Throws<AggregateException>(() =>
            _serviceUnderTest.AddCompetitors(eventId, [competitorId]).Wait());
        Assert.That(ex.InnerException, Is.TypeOf<ItemExistsException>());
    }

    [Test]
    public async Task AddSegments_AddsSegment()
    {
        await AddUser();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        var segmentList = new List<ulong> { 20, 30 };
        await _serviceUnderTest.AddSegments(createdEvent.Id, segmentList);

        segmentList.Add(10);

        var allSegments = (await _context.TryGetEvent(createdEvent.Id)).Segments;

        Assert.That(allSegments.Select(x => x.Id), Is.EquivalentTo(segmentList));
    }

    [Test]
    public async Task AddSegments_AllSegmentsAdded_ThrowItemExistsException()
    {
        await AddUser();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        var segmentList = new List<ulong> { 20, 30 };
        await _serviceUnderTest.AddSegments(createdEvent.Id, segmentList);

        var ex = Assert.Throws<AggregateException>(() =>
            _serviceUnderTest.AddSegments(createdEvent.Id, segmentList).Wait());
        Assert.That(ex.InnerException, Is.TypeOf<ItemExistsException>());

        segmentList.Add(10);

        var allSegments = (await _context.TryGetEvent(createdEvent.Id)).Segments;

        Assert.That(allSegments.Select(x => x.Id), Is.EquivalentTo(segmentList));
    }

    [Test]
    public async Task AddSegments_WhenSegmentNotInDB_InvokesPullMethod()
    {
        await AddUser();
        _segmentClient.Setup(x => x.PullSegment(It.IsAny<ulong>()))
            .Returns((ulong val) => Task.FromResult(PreapreObject.CreateSegment(val))).Verifiable();

        await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        _segmentClient.Verify(x => x.PullSegment(It.IsAny<ulong>()), Times.Once);
    }

    [Test]
    public async Task AddSegments_WhenSegmentInDB_DoesNotInvokePullMethod()
    {
        await AddUser();
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        await _context.SaveChangesAsync();

        _segmentClient.Setup(x => x.PullSegment(It.IsAny<ulong>()))
            .Returns((ulong val) => Task.FromResult(PreapreObject.CreateSegment(val))).Verifiable();

        await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        _segmentClient.Verify(x => x.PullSegment(It.IsAny<ulong>()), Times.Never);
    }

    [Test]
    public async Task AddResult_AddsResult()
    {
        await AddUser();
        ulong segmentId = 10;
        uint time = 101;
        _context.Segments.Add(PreapreObject.CreateSegment(segmentId));
        _context.UsersWithEvents.Add(new UserWithEvent { EventId = 1, UserId = 1 });
        await _context.SaveChangesAsync();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        await _serviceUnderTest.AddResult(1, createdEvent.Id, segmentId, time);

        Result result = null!;
        Assert.DoesNotThrow(() => result = _context.Results.Single(x => x.EventId == createdEvent.Id));
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Time, Is.EqualTo(time));
            Assert.That(result.UserId, Is.EqualTo(1));
            Assert.That(result.SegmentId, Is.EqualTo(segmentId));
        });
    }

    [Test]
    public async Task RemoveCompetitors_RemovesCompetitorsFromEvent()
    {
        await AddUser();
        await AddUser(2);
        await AddUser(3);
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        _context.Segments.Add(PreapreObject.CreateSegment(11));
        await _context.SaveChangesAsync();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);
        createdEvent.Competitors.AddRange(_context.Users.ToList());
        await _context.SaveChangesAsync();

        await _serviceUnderTest.RemoveCompetitors(createdEvent.Id, [2, 3]);

        var resultEvent = await _context.TryGetEvent(createdEvent.Id);

        Assert.That(resultEvent.Competitors.Select(c => c.Id), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task RemoveCompetitors_DoesNotRemoveUsers()
    {
        await AddUser();
        await AddUser(2);
        await AddUser(3);
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        _context.Segments.Add(PreapreObject.CreateSegment(11));
        await _context.SaveChangesAsync();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);
        createdEvent.Competitors.AddRange(_context.Users.ToList());
        await _context.SaveChangesAsync();

        await _serviceUnderTest.RemoveCompetitors(createdEvent.Id, [2, 3]);


        Assert.That(_context.Users.Select(u => u.Id), Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public async Task RemoveCompetitors_WhenUserIdNotInEvent_NothingChanges()
    {
        await AddUser();
        await AddUser(2);
        await AddUser(3);
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        _context.Segments.Add(PreapreObject.CreateSegment(11));
        await _context.SaveChangesAsync();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);
        createdEvent.Competitors.AddRange(_context.Users.Where(u => u.Id != 3).ToList());
        await _context.SaveChangesAsync();

        await _serviceUnderTest.RemoveCompetitors(createdEvent.Id, [3]);

        var resultEvent = await _context.TryGetEvent(createdEvent.Id);

        Assert.That(resultEvent.Competitors.Select(c => c.Id), Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public async Task RemoveCompetitors_WhenEventIdNotExist_ThrowNotFoundException()
    {
        await AddUser();
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        await _context.SaveChangesAsync();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);
        createdEvent.Competitors.AddRange(_context.Users.Where(u => u.Id == 1).ToList());
        await _context.SaveChangesAsync();

        Assert.ThrowsAsync<NotFoundException>(() => _serviceUnderTest.RemoveCompetitors(2, [1]));

        var resultEvent = await _context.TryGetEvent(createdEvent.Id);

        Assert.That(resultEvent.Competitors.Select(c => c.Id), Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public async Task RemoveSegments_RemovesSegmentsFromEvent()
    {
        await AddUser();
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        _context.Segments.Add(PreapreObject.CreateSegment(11));
        _context.Segments.Add(PreapreObject.CreateSegment(12));
        await _context.SaveChangesAsync();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);
        createdEvent.Segments.AddRange(_context.Segments.Where(s => s.Id != 10));
        await _context.SaveChangesAsync();

        await _serviceUnderTest.RemoveSegments(createdEvent.Id, [10, 11]);

        var resultEvent = await _context.TryGetEvent(createdEvent.Id);

        Assert.That(resultEvent.Segments.Select(c => c.Id), Is.EquivalentTo(new[] { 12 }));
    }

    [Test]
    public async Task RemoveSegments_DoesNotRemoveSegments()
    {
        await AddUser();
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        _context.Segments.Add(PreapreObject.CreateSegment(11));
        _context.Segments.Add(PreapreObject.CreateSegment(12));
        await _context.SaveChangesAsync();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);
        createdEvent.Segments.AddRange(_context.Segments.Where(s => s.Id != 10));
        await _context.SaveChangesAsync();

        await _serviceUnderTest.RemoveSegments(createdEvent.Id, [10, 11]);

        Assert.That(_context.Segments.Select(s => s.Id), Is.EquivalentTo(new[] { 10, 11, 12 }));
    }

    [Test]
    public async Task RemoveSegments_WhenSegmentIdNotInEvent_NothingChanges()
    {
        await AddUser();
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        _context.Segments.Add(PreapreObject.CreateSegment(11));
        await _context.SaveChangesAsync();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);
        createdEvent.Segments.AddRange(_context.Segments.Where(s => s.Id != 10));
        _context.Segments.Add(PreapreObject.CreateSegment(12));
        await _context.SaveChangesAsync();

        await _serviceUnderTest.RemoveSegments(createdEvent.Id, [12]);

        var resultEvent = await _context.TryGetEvent(createdEvent.Id);

        Assert.That(resultEvent.Segments.Select(s => s.Id), Is.EquivalentTo(new[] { 10, 11 }));
    }

    [Test]
    public async Task RemoveSegments_WhenEventIdNotExist_ThrowNotFoundException()
    {
        await AddUser();
        _context.Segments.Add(PreapreObject.CreateSegment(10));
        await _context.SaveChangesAsync();
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);
        createdEvent.Competitors.AddRange(_context.Users.Where(u => u.Id == 1).ToList());
        await _context.SaveChangesAsync();

        Assert.ThrowsAsync<NotFoundException>(() => _serviceUnderTest.RemoveSegments(2, [1]));

        var resultEvent = await _context.TryGetEvent(createdEvent.Id);

        Assert.That(resultEvent.Segments.Select(c => c.Id), Is.EqualTo(new[] { 10 }));
    }
}