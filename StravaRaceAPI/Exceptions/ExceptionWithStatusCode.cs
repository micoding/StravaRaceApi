using System.Net;

namespace StravaRaceAPI.Exceptions;

public abstract class ExceptionWithStatusCode : Exception
{
    public ExceptionWithStatusCode(string message) : base(message)
    {
    }

    public abstract HttpStatusCode StatusCode { get; protected set; }
}