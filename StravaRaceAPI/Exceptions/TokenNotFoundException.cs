namespace StravaRaceAPI.Exceptions;

public class TokenNotFoundException : NotFoundException
{
    public TokenNotFoundException(string message) : base(message)
    {
    }
}