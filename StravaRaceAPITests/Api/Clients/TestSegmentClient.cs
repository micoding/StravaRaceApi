using System.Net;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using StravaRaceAPI.Api;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;

namespace StravaRaceAPITests.Api.Clients;

public class TestSegmentClient : MockStravaAPI
{
    private SegmentClient _clientUnderTest;
    private ApiDBContext _context;
    private DbContextOptions<ApiDBContext> _options;
    
    private readonly Segment _segment = new()
    {
        Id = 1,
        Name = "Test Segment",
        Distance = 100,
        Elevation = 12.0f
    };
    
    private void Setup(HttpResponseMessage response)
    {
        _options = new DbContextOptionsBuilder<ApiDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApiDBContext(_options);
        
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
            new SegmentClient(MockTokenHandlerContext.Object, MockLogger.Object, MockHttpClientFactory.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task PullSegment_ReturnsSegment()
    {
        Setup(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(JsonConvert.SerializeObject(_segment)) });
        var result = await _clientUnderTest.PullSegment(1);
        
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(_segment.Id));
            Assert.That(result.Name, Is.EqualTo(_segment.Name));
            Assert.That(result.Distance, Is.EqualTo(_segment.Distance));
            Assert.That(result.Elevation, Is.EqualTo(_segment.Elevation));
        });
    }
    
    [Test]
    public void PullSegment_WhenResponseNotSuccess_ThrowsApiCommunicationException()
    {
        Setup(new HttpResponseMessage(HttpStatusCode.Forbidden));
        
        Assert.ThrowsAsync<ApiCommunicationError>(() => _clientUnderTest.PullSegment(1));
    }
    
    [Test]
    public async Task GetStarredSegments_ReturnsStarredSegment()
    {
        Setup(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(JsonConvert.SerializeObject(new List<Segment> { _segment })) });
        var result = await _clientUnderTest.GetStarredSegments();
        
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.First().Id, Is.EqualTo(_segment.Id));
            Assert.That(result.First().Name, Is.EqualTo(_segment.Name));
            Assert.That(result.First().Distance, Is.EqualTo(_segment.Distance));
            Assert.That(result.First().Elevation, Is.EqualTo(_segment.Elevation));
        });
    }
    
    [Test]
    public void GetStarredSegments_WhenResponseNotSuccess_ThrowsApiCommunicationException()
    {
        Setup(new HttpResponseMessage(HttpStatusCode.Forbidden));
        
        Assert.ThrowsAsync<ApiCommunicationError>(() => _clientUnderTest.GetStarredSegments());
    }
}