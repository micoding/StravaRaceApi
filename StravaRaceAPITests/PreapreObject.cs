using StravaRaceAPI.Entities;
using StravaRaceAPI.Models;

namespace StravaRaceAPITests;

public static class PreapreObject
{
    public static CreateEventDTO CreateEventDTO => new()
    {
        Name = "Name",
        Description = "Description",
        StartDate = DateTime.Now + TimeSpan.FromHours(1),
        EndDate = DateTime.Now + TimeSpan.FromDays(1),
        CreationDate = DateTime.Now,
        CreatorId = 1,
        SegmentIds = new List<ulong> { 10 }
    };

    public static Event CreateEvent(ulong id)
    {
        return new Event
        {
            Id = id,
            Name = "Name",
            Description = "Description",
            StartDate = DateTime.Now + TimeSpan.FromHours(1),
            EndDate = DateTime.Now + TimeSpan.FromDays(1),
            CreationDate = DateTime.Now,
            CreatorId = 1,
            Segments = new List<Segment> { CreateSegment(10) }
        };
    }

    public static Segment CreateSegment(ulong id)
    {
        return new Segment
        {
            Id = id,
            Name = "Test",
            Distance = 100,
            Elevation = 10,
            Events = new List<Event>(),
            Results = new List<Result>()
        };
    }

    public static User CreateUser(int id)
    {
        return new User
        {
            Id = id,
            FirstName = "FirstName",
            LastName = "LastName",
            Username = "Username",
            Email = "Email",
            ProfilePictureUrl = "ProfilePictureUrl",
            Token = new Token
            {
                UserId = id,
                ExpirationOfToken = DateTime.Now.AddHours(2),
                AccessToken = "AccessToken",
                RefreshToken = "RefreshToken"
            }
        };
    }
}