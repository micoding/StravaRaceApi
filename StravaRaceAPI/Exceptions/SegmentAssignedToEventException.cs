namespace StravaRaceAPI.Exceptions;

public class SegmentAssignedToEventException : ExceptionWithStatusCode
{
    public SegmentAssignedToEventException(string message) : base(message)
    {
    }

    public override HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.BadRequest;
}