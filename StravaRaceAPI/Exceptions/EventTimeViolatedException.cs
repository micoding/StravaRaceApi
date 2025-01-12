namespace StravaRaceAPI.Exceptions;

public class EventTimeViolatedException : ExceptionWithStatusCode
{
    public EventTimeViolatedException(string message) : base(message)
    {
    }

    public override HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.Forbidden;
}