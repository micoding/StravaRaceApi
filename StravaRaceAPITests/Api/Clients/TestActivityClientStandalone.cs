using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using StravaRaceAPI.Api;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Models;

namespace StravaRaceAPITests.Api.Clients;

public class TestActivityClientStandalone
{
    private ActivityClientStandalone _clientUnderTest;
    private Mock<ILogger<ActivityClientStandalone>> _mockLogger = new Mock<ILogger<ActivityClientStandalone>>();
    private Mock<ITokenHandlerInject> _mockTokenHandlerInject = new Mock<ITokenHandlerInject>();
    private Mock<IHttpClientFactory> _mockHttpClientFactory = new Mock<IHttpClientFactory>();
    
    private StravaActivity _activity = new StravaActivity
    {
        Id = 1,
        Name = "Test Activity",
        Manual = false,
        Private = false,
        SegmentsEfforts = new List<StravaSegmentEffort>
        {
            new StravaSegmentEffort()
            {
                ElapsedTime = 101,
                Segment = new Segment(),
                StartDate = DateTime.Today
            }
        },
        StartDate = DateTime.Today,
        Type = "ride"
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

        _mockHttpClientFactory.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(httpClient).Verifiable();

        _mockTokenHandlerInject = new Mock<ITokenHandlerInject>();

        _clientUnderTest =
            new ActivityClientStandalone(_mockHttpClientFactory.Object, _mockLogger.Object, _mockTokenHandlerInject.Object);
    }

    [Test]
    public async Task GetActivityById_ReturnsActivity()
    {
        Setup(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(JsonConvert.SerializeObject(_activity)) });
        
        var result= await _clientUnderTest.GetActivityById(1);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(_activity.Id));
            Assert.That(result.Name, Is.EqualTo(_activity.Name));
            Assert.That(result.Manual, Is.EqualTo(_activity.Manual));
            Assert.That(result.Private, Is.EqualTo(_activity.Private));
            Assert.That(result.Type, Is.EquivalentTo(_activity.Type));
            Assert.That(result.StartDate, Is.EqualTo(_activity.StartDate));
        });
    }
    
    [Test]
    public async Task GetActivityById_WhenResponseNotSuccess_ThrowsApiCommunicationError()
    {
        Setup(new HttpResponseMessage(HttpStatusCode.Forbidden));

        Assert.ThrowsAsync<ApiCommunicationError>(() => _clientUnderTest.GetActivityById(1));
    }
}