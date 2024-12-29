using StravaRaceAPI.Entities;
using StravaRaceAPI.Models;

namespace StravaRaceAPITests;

public static class PreapreObject
{
    public static Segment CreateSegment => new()
    {
        Id = 10,
        Name = "Test",
        Distance = 100,
        Elevation = 10,
        Events = new List<Event>(),
        Results = new List<Result>()
    };

    public static CreateEventDTO CreateEventDTO => new()
    {
        Name = "Name",
        Description = "Description",
        StartDate = DateTime.Now + TimeSpan.FromHours(1),
        EndDate = DateTime.Now + TimeSpan.FromDays(1),
        CreationDate = DateTime.Now,
        CreatorId = 1,
        SegmentIds = new List<int> { 10, 100 }
    };

    public static User CreateUser => new()
    {
        Id = 1,
        FirstName = "FirstName",
        LastName = "LastName",
        Username = "Username",
        Email = "Email",
        Token = new Token
        {
            UserId = 1,
            ExpirationOfToken = DateTime.Now.AddHours(2),
            AccessToken = "AccessToken",
            RefreshToken = "RefreshToken"
        }
    };
}