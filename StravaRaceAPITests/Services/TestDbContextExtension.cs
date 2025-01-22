using Microsoft.EntityFrameworkCore;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Services;

namespace StravaRaceAPITests.Services;

public class TestDbContextExtension
{
    private ApiDBContext _context;
    private DbContextOptions<ApiDBContext> _options;

    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<ApiDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApiDBContext(_options);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private async Task AddUser(int id = 1)
    {
        var user = PreapreObject.CreateUser(id);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    [Test]
    public async Task AddResult_AddedSuccessfully()
    {
        await AddUser();
        _context.Events.Add(PreapreObject.CreateEvent(1));
        _context.UsersWithEvents.Add(new UserWithEvent { EventId = 1, UserId = 1 });
        await _context.SaveChangesAsync();
        uint time = 101;

        await _context.AddResult(1, 1, 10, time);

        Assert.That(_context.Results.Count, Is.EqualTo(1));
        Assert.That(_context.Results.First(r => r.UserId == 1 && r.SegmentId == 10 && r.EventId == 1).Time,
            Is.EqualTo(time));
    }

    [Test]
    public async Task AddResult_UserNotExist_ThrowsException()
    {
        await AddUser();
        _context.Events.Add(PreapreObject.CreateEvent(1));
        await _context.SaveChangesAsync();
        uint time = 101;

        Assert.ThrowsAsync<NotFoundException>(() => _context.AddResult(2, 1, 10, time));
    }

    [Test]
    public async Task AddResult_EventNotExist_ThrowsException()
    {
        await AddUser();
        _context.Events.Add(PreapreObject.CreateEvent(1));
        _context.UsersWithEvents.Add(new UserWithEvent { EventId = 1, UserId = 1 });
        await _context.SaveChangesAsync();
        uint time = 101;

        Assert.ThrowsAsync<NotFoundException>(() => _context.AddResult(1, 2, 10, time));
    }

    [Test]
    public async Task AddResult_UserNotEnrolled_ThrowsException()
    {
        await AddUser();
        await AddUser(2);
        _context.Events.Add(PreapreObject.CreateEvent(1));
        _context.UsersWithEvents.Add(new UserWithEvent { EventId = 1, UserId = 1 });
        await _context.SaveChangesAsync();
        uint time = 101;

        Assert.ThrowsAsync<NotFoundException>(() => _context.AddResult(2, 1, 10, time));
    }

    [Test]
    public async Task AddResult_SegmentNotInEvent_ThrowsException()
    {
        await AddUser();
        _context.Events.Add(PreapreObject.CreateEvent(1));
        _context.UsersWithEvents.Add(new UserWithEvent { EventId = 1, UserId = 1 });
        _context.Segments.Add(PreapreObject.CreateSegment(11));
        await _context.SaveChangesAsync();
        uint time = 101;

        Assert.ThrowsAsync<NotFoundException>(() => _context.AddResult(1, 1, 11, time));
    }

    [Test]
    public async Task AddResult_ResultAlreadyExist_ThrowsException()
    {
        await AddUser();
        _context.Events.Add(PreapreObject.CreateEvent(1));
        _context.UsersWithEvents.Add(new UserWithEvent { EventId = 1, UserId = 1 });
        _context.Results.Add(new Result { EventId = 1, SegmentId = 10, Time = 99, UserId = 1 });
        await _context.SaveChangesAsync();
        uint time = 101;

        Assert.ThrowsAsync<ItemExistsException>(() => _context.AddResult(1, 1, 10, time));
    }

    [Test]
    public async Task UserExists_UserExist_ReturnsTrue()
    {
        await AddUser();

        var answer = await _context.UsersExist([1]);

        Assert.That(answer, Is.True);
    }

    [Test]
    public async Task UserExists_UserNotExist_ReturnsFalse()
    {
        var answer = await _context.UsersExist([1]);

        Assert.That(answer, Is.False);
    }
}