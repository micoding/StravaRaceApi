namespace StravaRaceAPI.Api;

public interface ITokenHandlerContext : ITokenHandler;

public class TokenHandlerContext : TokenHandler, ITokenHandlerContext
{
    public TokenHandlerContext(IUserContextService userService, ApiDBContext context) : base(context)
    {
        var user = context.Users.Include(u => u.Token).SingleOrDefault(u => u.Id == userService.GetUserId);
        var token = user?.Token;
        Token = token ?? throw new NotFoundException($"Token of the user: {userService.GetUserId} not found!");
    }
}