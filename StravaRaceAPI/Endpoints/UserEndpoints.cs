using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Models;
using StravaRaceAPI.Services;

namespace StravaRaceAPI.Endpoints;

public static class UserEndpoints
{
    /// <summary>
    ///     User related endpoint registration method.
    /// </summary>
    /// <param name="app" cref="WebApplication">WebApplication.</param>
    /// <returns cref="WebApplication">WebApplication.</returns>
    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("user/{id:int}", GetUserById)
            .Produces<AthleteDTO>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        app.MapGet("user/starred", GetUserStarred)
            .Produces<SegmentDTO>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .RequireAuthorization("LoggedIn");

        app.MapPost("connectWithStrava", ConnectWithStrava)
            .Produces<string>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<IResult> GetUserById(ApiDBContext db, [FromRoute] int id, IAthleteClient athleteClient)
    {
        var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
            throw new NotFoundException($"User with id {id} not found");

        var athlete = await athleteClient.GetAthleteAsync();
        return Results.Ok(athlete);
    }

    private static async Task<IResult> GetUserStarred(ApiDBContext db, ISegmentClient segmentClient,
        IUserContextService userContext)
    {
        var id = userContext.GetUserId;
        var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
            throw new NotFoundException($"User with id {id} not found");

        var starred = await segmentClient.GetStarredSegmentsAsync();
        return Results.Ok(starred);
    }

    private static async Task<IResult> ConnectWithStrava([FromBody] object response, IMapper mapper,
        IUserService service)
    {
        var output = JsonSerializer.Deserialize<ConnnectWithStravaDTO>(response.ToString() ?? string.Empty);
        var userDto = output?.athlete;
        var tokenDto = JsonSerializer.Deserialize<TokenDTO>(response.ToString() ?? string.Empty);
        if (userDto is null || tokenDto is null) throw new ApiCommunicationError("Invalid response");

        var user = mapper.Map<User>(userDto);
        var tokenApi = mapper.Map<Token>(tokenDto);

        tokenApi.User = user;
        user.Token = tokenApi;

        var token = await service.SignInWithStrava(user);

        return Results.Ok(token);
    }
}