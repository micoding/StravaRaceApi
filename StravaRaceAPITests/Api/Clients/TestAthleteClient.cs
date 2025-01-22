using System.Net;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using StravaRaceAPI.Api;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Models;

namespace StravaRaceAPITests.Api.Clients;

[TestFixture]
[TestOf(typeof(StravaRaceAPI.Api.Clients.TestAthleteClient))]
public class TestAthleteClient : MockStravaAPI
{
    private StravaRaceAPI.Api.Clients.TestAthleteClient _clientUnderTest;

    private readonly AthleteDTO _athlete = new()
    {
        Id = 1,
        FirstName = "John",
        LastName = "Doe",
        Username = "john_doe",
        PhotoUrl = "https://www.strava.com/photo/200?id=1",
        Sex = "m"
    };


    private void Setup(HttpResponseMessage response)
    {
        var clientHandlerMock = new Mock<HttpMessageHandler>();
        clientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();
        clientHandlerMock.As<IDisposable>().Setup(s => s.Dispose());

        var httpClient = new HttpClient(clientHandlerMock.Object);

        MockHttpClientFactory.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(httpClient).Verifiable();

        MockTokenHandlerContext = new Mock<ITokenHandlerContext>();

        _clientUnderTest =
            new StravaRaceAPI.Api.Clients.TestAthleteClient(MockTokenHandlerContext.Object, MockLogger.Object, MockHttpClientFactory.Object);
    }

    [Test]
    public async Task GetAthleteAsync_ReturnsAthlete()
    {
        Setup(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(JsonConvert.SerializeObject(_athlete)) });

        var returned = await _clientUnderTest.GetAthleteAsync();

        Assert.That(returned, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(returned.Id, Is.EqualTo(_athlete.Id));
            Assert.That(returned.FirstName, Is.EqualTo(_athlete.FirstName));
            Assert.That(returned.LastName, Is.EqualTo(_athlete.LastName));
            Assert.That(returned.Username, Is.EqualTo(_athlete.Username));
            Assert.That(returned.PhotoUrl, Is.EqualTo(_athlete.PhotoUrl));
            Assert.That(returned.Sex, Is.EqualTo(_athlete.Sex));
        });
    }

    [Test]
    public void GetAthleteAsync_WhenResponseNotSuccess_ThrowsApiCommunicationError()
    {
        Setup(new HttpResponseMessage(HttpStatusCode.Forbidden));

        Assert.ThrowsAsync<ApiCommunicationError>(() => _clientUnderTest.GetAthleteAsync());
    }
}