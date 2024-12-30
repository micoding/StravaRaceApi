using Microsoft.AspNetCore.Authorization;

namespace StravaRaceAPI.Authorization;

public class AuthorizationRequirementHandler : AuthorizationHandler<AuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationRequirement requirement)
    {
        throw new NotImplementedException();
    }
}