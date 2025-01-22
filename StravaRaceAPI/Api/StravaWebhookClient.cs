namespace StravaRaceAPI.Api;

public interface IStravaWebhookClient
{
    Task AssignToWebhook();
    Task<string> ValidateSubscriptionRequest(ApiWebhookSubscriptionValidationDTO subscriptionValidationDTO);
}

public class StravaWebhookClient : IStravaWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StravaWebhookClient> _logger;

    public StravaWebhookClient(IHttpClientFactory httpClientFactory, ILogger<StravaWebhookClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("HttpClient");
        _logger = logger;
    }

    public async Task AssignToWebhook()
    {
        var response = await _httpClient.PostAsync(ApiConfiguration.Current.GetWebhookSubscribe(), null);
        if (!response.IsSuccessStatusCode)
            throw new ApiCommunicationError($"Communication Error: {response.ReasonPhrase}");
    }

    public async Task<string> ValidateSubscriptionRequest(ApiWebhookSubscriptionValidationDTO subscriptionValidationDTO)
    {
        if (subscriptionValidationDTO.HubMode is not "subscribe")
            throw new ApiCommunicationError("Webhook Subscription Validation Failed - Not: subscribe");

        if (subscriptionValidationDTO.HubVerifyToken == ApiConfiguration.Current.WebhookVerifyToken)
            throw new ApiCommunicationError("Webhook Subscription Validation Failed - Not Authorized");

        return subscriptionValidationDTO.HubChallenge;
    }

    public async Task CheckSubscription(ApiWebhookSubscriptionValidationDTO subscriptionValidationDTO)
    {
    }
}