using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using StravaRaceAPI;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Services;

namespace StravaRaceAPITests;

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
            .Returns(Task.FromResult(PreapreObject.CreateSegment));

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
            .Returns(Task.FromResult(PreapreObject.CreateSegment)).Verifiable();

        await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        _segmentClient.Verify(x => x.PullSegment(It.IsAny<ulong>()), Times.Once);
    }

    [Test]
    public async Task CreateEvent_WhenSegmentInDB_DoesNotInvokePullMethod()
    {
        await AddUser();
        _context.Segments.Add(PreapreObject.CreateSegment);
        await _context.SaveChangesAsync();

        _segmentClient.Setup(x => x.PullSegment(It.IsAny<ulong>()))
            .Returns(Task.FromResult(PreapreObject.CreateSegment)).Verifiable();

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

        var ex = Assert.Throws<AggregateException>(() => _serviceUnderTest.AddCompetitors(eventId, [competitorId]).Wait());
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
    public async Task AddCompetitor_UserNotExist_ThrowCompetitorAssignedException()
    {
        await AddUser();
        await AddUser(2);
        const int eventId = 1;
        const int competitorId = 2;
        var createdEvent = await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);

        createdEvent.Competitors.Add(_context.Users.First(x => x.Id == competitorId));
        await _context.SaveChangesAsync();

        var ex = Assert.Throws<AggregateException>(() => _serviceUnderTest.AddCompetitors(eventId, [competitorId]).Wait());
        Assert.That(ex.InnerException, Is.TypeOf<CompetitorAssignedToEventException>());
    }
}