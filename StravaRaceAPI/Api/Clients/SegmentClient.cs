using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Models;

namespace StravaRaceAPI.Api.Clients;

public interface ISegmentClient
{
    Task<List<SegmentDTO>> GetStarredSegmentsAsync();
    Task<Segment> PullSegment(int segmentId);
}

public class SegmentClient : StravaApiClient, ISegmentClient
{
    public SegmentClient(ITokenHandler tokenHandler, ILogger<StravaApiClient> logger) : base(tokenHandler, logger)
    {
    }

    /// <summary>
    ///     Get Starred segments of the currently authenticated athlete.
    /// </summary>
    /// <returns>List of all starred users segments.</returns>
    /// <exception cref="ApiCommunicationError">When response fails.</exception>
    /// <exception cref="NotFoundException">When no starred segment found.</exception>
    public async Task<List<SegmentDTO>> GetStarredSegmentsAsync()
    {
        var response = await HttpClient.GetAsync(Endpoints.SegmentsStarred);
        if (!response.IsSuccessStatusCode)
            throw new ApiCommunicationError($"Communication Error: {response.ReasonPhrase}");

        var segments = await response.Content.ReadFromJsonAsync<List<SegmentDTO>>() ??
                       throw new NotFoundException("No starred segments found.");

        Logger.LogInformation(await response.Content.ReadAsStringAsync());

        return segments;
    }

    /// <summary>
    ///     Get the Segment by its id.
    /// </summary>
    /// <param name="segmentId">Id of the segment to get.</param>
    /// <returns>Segment with given segmentId.</returns>
    /// <exception cref="ApiCommunicationError">When response fails.</exception>
    /// <exception cref="NotFoundException">When no segment found.</exception>
    public async Task<Segment> PullSegment(int segmentId)
    {
        var response = await HttpClient.GetAsync(Endpoints.Segment + $"/{segmentId}");
        if (!response.IsSuccessStatusCode)
            throw new ApiCommunicationError($"Communication Error: {response.ReasonPhrase}");

        var segment = await response.Content.ReadFromJsonAsync<Segment>() ??
                      throw new NotFoundException($"Segment with id: {segmentId} not found.");

        Logger.LogInformation(await response.Content.ReadAsStringAsync());

        return segment;
    }
}