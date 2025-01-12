using System.Net;
using Moq;
using Moq.Protected;
using StravaRaceAPI.Api.Clients;

namespace StravaRaceAPITests.Api.Clients;

[TestFixture]
[TestOf(typeof(AthleteClient))]
public class AthleteClientTest : MockStravaAPI
{
    [SetUp]
    public void Setup()
    {
        var clientHandlerMock = new Mock<DelegatingHandler>();
        clientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .Verifiable();
        clientHandlerMock.As<IDisposable>().Setup(s => s.Dispose());

        var httpClient = new HttpClient(clientHandlerMock.Object);

        MockHttpClientFactory.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(httpClient).Verifiable();


        // MockHttpClientFactory.Setup(x => x.)
        // _client = new AthleteClient(MockTokenHandler.Object, MockLogger.Object);
    }

    [Test]
    public void GetAthleteAsync_WhenCalled_ReturnsAthlete()
    {
        // MockHttpClientFactory.Verify(cf => cf.CreateClient());
        // MockHttpClientFactory.Protected().Verify("SendAsync", Times.Exactly(1), ItExpr.IsAny<HttpRequestMessage>(),
        //     ItExpr.IsAny<CancellationToken>());
    }
}