namespace StravaRaceAPI.Exceptions;

public class ItemExistsException : ExceptionWithStatusCode
{
    public ItemExistsException(string message) : base(message)
    {
    }

    public override HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.BadRequest;
}