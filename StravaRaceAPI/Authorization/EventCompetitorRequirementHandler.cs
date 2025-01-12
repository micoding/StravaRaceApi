using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using StravaRaceAPI.Entities;

namespace StravaRaceAPI.Authorization;

public class EventCompetitorRequirementHandler : AuthorizationHandler<EventCompetitorRequirement, Event>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EventCompetitorRequirement requirement,
        Event eventToCheck)
    {
        var userId = context.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        if (eventToCheck.Competitors.Any(c => c.Id == int.Parse(userId)))
            context.Succeed(requirement);
        
        return Task.CompletedTask;
    }
}