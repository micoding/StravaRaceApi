namespace StravaRaceAPI.Exceptions;

public class CanNotReadException : ExceptionWithStatusCode
{
    public CanNotReadException(string message) : base(message)
    {
    }

    public override HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.InternalServerError;
}