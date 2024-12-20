using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StravaRaceAPI;
using StravaRaceAPI.Api;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Models;
using StravaRaceAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var apiConfiguration = new ApiConfiguration();
ApiConfiguration.Current = apiConfiguration;
builder.Configuration.GetSection(nameof(ApiConfiguration)).Bind(apiConfiguration);
builder.Services.AddSingleton(apiConfiguration);
builder.Services.AddAutoMapper(typeof(StravaMappingProfile));
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddDbContext<ApiDBContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("StravaRaceConnectionString"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("StravaRaceConnectionString")));
});

var app = builder.Build();

app.MapGet("user/{id:int}", async (ApiDBContext db, int id) =>
    {
        var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
            throw new NotFoundException($"User with id {id} not found");

        var ac = new AthleteClient(new TokenHandler(user.Token, db));

        //var athlete = await ac.GetAthleteAsync();
    })
    .Produces<AthleteDTO>()
    .Produces(StatusCodes.Status404NotFound);

app.MapGet("user/{id:int}/starred/", async (ApiDBContext db, int id) =>
    {
        var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
            throw new NotFoundException($"User with id {id} not found");

        var ac = new SegmentClient(new TokenHandler(user.Token, db));

        var starred = await ac.GetStarredSegmentsAsync();
        return Results.Ok(starred);
    })
    .Produces<SegmentDTO>()
    .Produces(StatusCodes.Status404NotFound);

app.MapPost("event", async (ApiDBContext db, [FromBody] CreateEventDTO dto, IEventService service) =>
    {
        var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Id == dto.CreatorId);
        if (user is null)
            throw new NotFoundException($"User with id {dto.CreatorId} not found");

        var newEvent = await service.CreateEvent(dto);

        return Results.Ok(newEvent);
    })
    .Produces<SegmentDTO>()
    .Produces(StatusCodes.Status404NotFound);

app.MapGet("connectWithStrava", async (ApiDBContext db, HttpClient client, IMapper mapper, IUserService service) =>
    {
        var redirect = await client.GetAsync(Endpoints.AuthorizeCode);
        if (!redirect.IsSuccessStatusCode) throw new ApiCommunicationError("Could not connect to strava");

        var code = HttpUtility.ParseQueryString(redirect.Headers.Location.AbsoluteUri).Get("code");

        if (code is null) throw new ApiCommunicationError("Invalid code");

        var response = await client.PostAsync(apiConfiguration.GetAuthorizationCode(), null);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get token: {response.StatusCode}");

        var userDto = await response.Content.ReadFromJsonAsync<AthleteDTO>();
        var tokenDto = await response.Content.ReadFromJsonAsync<TokenDTO>();
        if (userDto is null || tokenDto is null) throw new ApiCommunicationError("Invalid response");

        var user = mapper.Map<User>(userDto);
        var token = mapper.Map<Token>(tokenDto);

        user.Token = token;

        user = await service.SignInWithStrava(user);

        return Results.Ok(user);
    })
    .Produces<SegmentDTO>()
    .Produces(StatusCodes.Status404NotFound);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();