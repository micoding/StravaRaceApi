using StravaRaceAPI.Api.Clients;

namespace StravaRaceAPITests.Api.Clients;

[TestFixture]
[TestOf(typeof(AthleteClient))]
public class AthleteClientTest : MockStravaAPI
{
    [SetUp]
    public void Setup()
    {
        _client = new AthleteClient(MockTokenHandler.Object, MockLogger.Object);
    }

    private AthleteClient _client;

    [Test]
    public async Task GetAthleteAsync_WhenCalled_ReturnsAthlete()
    {
    }
}