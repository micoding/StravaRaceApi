using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using StravaRaceAPI.Api;
using StravaRaceAPI.Api.Clients;

namespace StravaRaceAPITests.Api.Clients;

public abstract class MockStravaAPI
{
    protected readonly Mock<ILogger<StravaApiClient>> MockLogger = new();
    protected readonly Mock<ITokenHandler> MockTokenHandler = new();
    protected readonly Mock<IHttpClientFactory> MockHttpClientFactory = new();
}