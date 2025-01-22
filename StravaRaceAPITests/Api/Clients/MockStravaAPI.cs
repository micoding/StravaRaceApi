using Microsoft.Extensions.Logging;
using Moq;
using StravaRaceAPI.Api;
using StravaRaceAPI.Api.Clients;

namespace StravaRaceAPITests.Api.Clients;

public abstract class MockStravaAPI
{
    protected readonly Mock<IHttpClientFactory> MockHttpClientFactory = new();
    protected readonly Mock<ILogger<StravaApiClient>> MockLogger = new();
    protected Mock<ITokenHandlerContext> MockTokenHandlerContext = new();
}