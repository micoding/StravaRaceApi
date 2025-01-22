namespace StravaRaceAPI.Endpoints;

public static class WebHookEndpoints
{
    public static WebApplication MapWebHooks(this WebApplication app)
    {
        app.MapGet("/webhook", ValidateSubscriptionRequest);
        app.MapPost("/webhook", HandleWebhook);

        return app;
    }

    private static async Task<IResult> ValidateSubscriptionRequest(IStravaWebhookClient client,
        [AsParameters] ApiWebhookSubscriptionValidationDTO validationDto)
    {
        var json = await client.ValidateSubscriptionRequest(validationDto);

        return Results.Ok(json);
    }

    private static async Task<IResult> HandleWebhook(ApiDBContext db, [FromBody] EventDataDTO eventData,
        IActivityClientStandalone client, ITokenHandlerInject tokenHandler)
    {
        if (eventData.ObjectType is not "activity") return Results.Ok();
        if (eventData.AspectType is not "create") return Results.Ok();

        var stravaActivityId = eventData.ObjectId;
        var timeOfTheEvent = new DateTime(eventData.EventTime);
        var user = await db.Users.Include(u => u.Token).Include(u => u.Events).Include(u => u.Events)
            .ThenInclude(e => e.Segments).FirstOrDefaultAsync(x => x.Id == eventData.AthleteId);
        if (user == null) return Results.Ok();

        tokenHandler.Token = user.Token;

        var openEvents = user.Events.Where(e => e.StartDate < timeOfTheEvent && e.EndDate > timeOfTheEvent).ToList();

        var activity = await client.GetActivityById(stravaActivityId);
        var efforts = activity.SegmentsEfforts.FindAll(effort =>
            openEvents.Any(e => e.Segments.Select(s => s.Id).Contains(effort.Segment.Id)));
        foreach (var effort in efforts)
        {
            var events = openEvents.Where(e => e.Id == effort.Segment.Id);
            foreach (var eventToMod in events)
                await db.AddResult(user.Id, eventToMod.Id, effort.Segment.Id, effort.ElapsedTime);
        }

        return Results.Ok();
    }
}