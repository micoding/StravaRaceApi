namespace StravaRaceAPI.Exceptions;

public class CompetitorAssignedToEventException : ExceptionWithStatusCode
{
    public CompetitorAssignedToEventException(string message) : base(message)
    {
    }

    public override HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.BadRequest;
}