namespace StravaRaceAPI.Authorization;

public class EventAuthorRequirementHandler : AuthorizationHandler<EventAuthorRequirement, Event>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        EventAuthorRequirement requirement, Event eventToCheck)
    {
        var userId = context.User.FindFirst(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        if (eventToCheck.CreatorId.ToString() == userId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}