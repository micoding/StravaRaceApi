namespace StravaRaceAPI.Api;

public interface ITokenHandlerInject : ITokenHandler
{
    Token Token { set; }
}

public class TokenHandlerInject : TokenHandler, ITokenHandlerInject
{
    public TokenHandlerInject(ApiDBContext context) : base(context)
    {
    }
}