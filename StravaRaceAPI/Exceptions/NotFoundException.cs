namespace StravaRaceAPI.Exceptions;

public class NotFoundException : ExceptionWithStatusCode
{
    public NotFoundException(string message) : base(message)
    {
    }

    public override HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.NotFound;
}