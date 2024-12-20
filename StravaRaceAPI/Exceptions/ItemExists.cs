using System.Net;

namespace StravaRaceAPI.Exceptions;

public class ItemExists : ExceptionWithStatusCode
{
    public ItemExists(string message) : base(message)
    {
    }

    public override HttpStatusCode StatusCode { get; protected set; } = HttpStatusCode.BadRequest;
}