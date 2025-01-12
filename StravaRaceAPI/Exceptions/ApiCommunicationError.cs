namespace StravaRaceAPI.Exceptions;

public class ApiCommunicationError : ExceptionWithStatusCode
{
    public ApiCommunicationError(string message) : base(message)
    {
    }

    public override HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.InternalServerError;
}