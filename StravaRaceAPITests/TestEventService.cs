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
        _segmentClient.Setup(r => r.PullSegment(It.IsAny<int>()))
            .Returns(Task.FromResult<Segment>(PreapreObject.CreateSegment));

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

    private async Task AddUser()
    {
        _context.Users.Add(PreapreObject.CreateUser);
        await _context.SaveChangesAsync();
    }

    [Test]
    public async Task CreateEvent_WhenSegmentNotInDB_InvokesPullMethod()
    {
        await AddUser();
        _segmentClient.Setup(x => x.PullSegment(It.IsAny<int>()))
            .Returns(Task.FromResult<Segment>(PreapreObject.CreateSegment)).Verifiable();
        
        await _serviceUnderTest.CreateEvent(PreapreObject.CreateEventDTO);
        
        _segmentClient.Verify(x => x.PullSegment(It.IsAny<int>()), Times.Once);
    }
}