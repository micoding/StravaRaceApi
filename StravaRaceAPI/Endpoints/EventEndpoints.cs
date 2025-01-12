using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StravaRaceAPI.Authorization;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Models;
using StravaRaceAPI.Services;

namespace StravaRaceAPI.Endpoints;

public static class EventEndpoints
{
    /// <summary>
    ///     Event related endpoint registration method.
    /// </summary>
    /// <param name="app" cref="WebApplication">WebApplication.</param>
    /// <returns cref="WebApplication">WebApplication.</returns>
    public static WebApplication MapEventEndpoints(this WebApplication app)
    {
        app.MapGet("events", GetAll)
            .Produces<List<AllEventDTO>>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        app.MapGet("event/{id}", GetById)
            .Produces<ShowEventDTO>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        app.MapPost("event", Create)
            .Produces<ShowEventDTO>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        app.MapPut("event/{id}/addCompetitors", AddCompetitors)
            .Produces<ShowEventDTO>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        app.MapDelete("event/{id}/removeCompetitors", RemoveCompetitors)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        app.MapPut("event/{id}/addSegments", AddSegments)
            .Produces<ShowEventDTO>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        app.MapDelete("event/{id}/removeSegments", RemoveSegments)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        app.MapPut("event/{id}/addResult", AddResult)
            .Produces<ShowEventDTO>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        return app;
    }

    private static async Task<IResult> GetAll(IEventService service, IMapper map)
    {
        var eventsToReturn = await service.GetAllEvents();
        var eventsDto = map.Map<List<AllEventDTO>>(eventsToReturn);
        return Results.Ok(eventsDto);
    }

    private static async Task<IResult> GetById([FromRoute] ulong id, IEventService service, IMapper map)
    {
        var eventToReturn = await service.GetEvent(id);
        var eventDto = map.Map<ShowEventDTO>(eventToReturn);
        return Results.Ok(eventDto);
    }

    private static async Task<IResult> Create(ApiDBContext db, [FromBody] CreateEventDTO dto, IEventService service,
        IMapper map)
    {
        var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Id == dto.CreatorId);
        if (user is null)
            throw new NotFoundException($"User with id {dto.CreatorId} not found");

        var newEvent = await service.CreateEvent(dto);
        var eventToShow = map.Map<ShowEventDTO>(newEvent);

        return Results.Ok(eventToShow);
    }

    private static async Task<IResult> AddCompetitors([FromRoute] ulong id, ApiDBContext db,
        [FromBody] ModifyEventCompetitorsDTO dto,
        IEventService service, IAuthorizationService authService, IUserContextService userContext)
    {
        if (dto.Competitors.Count is 0)
            throw new NotFoundException("No Competitor passed");

        var eventToModify = await db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventToModify is null) throw new NotFoundException($"Event with id {id} not found");
        if (eventToModify.StartDate < DateTime.Now)
            throw new EventTimeViolatedException("Event already started");

        var authResult =
            await authService.AuthorizeAsync(userContext.User, eventToModify, new EventAuthorRequirement());
        if (!authResult.Succeeded) return Results.Unauthorized();

        await service.AddCompetitors(id, dto.Competitors);

        return Results.Ok();
    }

    private static async Task<IResult> RemoveCompetitors([FromRoute] ulong id, ApiDBContext db,
        [FromBody] ModifyEventCompetitorsDTO dto, IEventService service, IAuthorizationService authService,
        IUserContextService userContext)
    {
        if (dto.Competitors.Count is 0)
            throw new NotFoundException("No Competitor passed");

        var eventToModify = await db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventToModify is null) throw new NotFoundException($"Event with id {id} not found");

        var authResult =
            await authService.AuthorizeAsync(userContext.User, eventToModify, new EventAuthorRequirement());
        if (!authResult.Succeeded) return Results.Unauthorized();

        await service.RemoveCompetitors(id, dto.Competitors);

        return Results.NoContent();
    }

    private static async Task<IResult> AddSegments([FromRoute] ulong id, ApiDBContext db,
        [FromBody] ModifyEventSegmentsDTO dto,
        IEventService service, IAuthorizationService authService, IUserContextService userContext)
    {
        if (dto.Segments.Count is 0)
            throw new NotFoundException("No Segments passed");

        var eventToModify = await db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventToModify is null) throw new NotFoundException($"Event with id {id} not found");
        if (eventToModify.StartDate < DateTime.Now)
            throw new EventTimeViolatedException("Event already started");

        var authResult =
            await authService.AuthorizeAsync(userContext.User, eventToModify, new EventAuthorRequirement());
        if (!authResult.Succeeded) return Results.Unauthorized();

        await service.AddSegments(id, dto.Segments);

        return Results.Ok();
    }

    private static async Task<IResult> RemoveSegments([FromRoute] ulong id, ApiDBContext db,
        [FromBody] ModifyEventSegmentsDTO dto,
        IEventService service, IAuthorizationService authService, IUserContextService userContext)
    {
        if (dto.Segments.Count is 0)
            throw new NotFoundException("No Segments passed");

        var eventToModify = await db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventToModify is null) throw new NotFoundException($"Event with id {id} not found");
        if (eventToModify.StartDate < DateTime.Now)
            throw new EventTimeViolatedException("Event already started");

        var authResult =
            await authService.AuthorizeAsync(userContext.User, eventToModify, new EventAuthorRequirement());
        if (!authResult.Succeeded) return Results.Unauthorized();

        await service.RemoveSegments(id, dto.Segments);

        return Results.NoContent();
    }

    private static async Task<IResult> AddResult([FromRoute] ulong id, ApiDBContext db, [FromBody] ResultDTO dto,
        IEventService service, IAuthorizationService authService, IUserContextService userContext)
    {
        var eventToModify = await db.Events.Include(e => e.Competitors).FirstOrDefaultAsync(e => e.Id == id);
        if (eventToModify is null) throw new NotFoundException($"Event with id {id} not found");
        if (eventToModify.StartDate > DateTime.Now)
            throw new EventTimeViolatedException("Event not started yet");
        if (eventToModify.EndDate < DateTime.Now) throw new EventTimeViolatedException("Event already ended");

        var authResult =
            await authService.AuthorizeAsync(userContext.User, eventToModify, new EventCompetitorRequirement());
        if (!authResult.Succeeded) return Results.Unauthorized();

        await service.AddResult((int)userContext.GetUserId!, id, dto.SegmentId, dto.Time);

        return Results.Ok();
    }
}