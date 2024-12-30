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
    public SegmentClient(TokenHandler token) : base(token)
    {
    }

    /// <summary>
    /// Get Starred segments of logged user.
    /// </summary>
    /// <returns>List of all starred users segemnts.</returns>
    /// <exception cref="NotFoundException">When no starred segment found.</exception>
    public async Task<List<SegmentDTO>> GetStarredSegmentsAsync()
    {
        List<SegmentDTO> segments = null!;
        var response = await HttpClient.GetAsync(Endpoints.SegmentsStarred);
        if (!response.IsSuccessStatusCode) return segments;
        
        segments = await response.Content.ReadFromJsonAsync<List<SegmentDTO>>() ??
                   throw new NotFoundException("No starred segments found.");
        var str = await response.Content.ReadAsStringAsync();

        return segments;
    }

    public async Task<Segment> PullSegment(int segmentId)
    {
        Segment segment = null;
        var response = await HttpClient.GetAsync(Endpoints.Segment + $"/{segmentId}");
        if (!response.IsSuccessStatusCode)
            throw new ApiCommunicationError($"Communication Error: {response.ReasonPhrase}");
        segment = await response.Content.ReadFromJsonAsync<Segment>() ??
                  throw new NotFoundException($"Segment with id: {segmentId} not found.");
        var str = await response.Content.ReadAsStringAsync();

        return segment;
    }
}