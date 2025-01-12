using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using StravaRaceAPI;
using StravaRaceAPI.Api;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Authorization;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Exceptions;
using StravaRaceAPI.Middlewares;
using StravaRaceAPI.Models;
using StravaRaceAPI.Services;
using Swashbuckle.AspNetCore.Filters;
using AuthenticationOptions = StravaRaceAPI.AuthenticationOptions;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using TokenHandler = StravaRaceAPI.Api.TokenHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var apiConfiguration = new ApiConfiguration();
ApiConfiguration.Current = apiConfiguration;
builder.Configuration.GetSection(nameof(ApiConfiguration)).Bind(apiConfiguration);
builder.Services.AddSingleton(apiConfiguration);
builder.Services.AddAutoMapper(typeof(StravaMappingProfile));
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<ISegmentClient, SegmentClient>();
builder.Services.AddScoped<IAthleteClient, AthleteClient>();
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<IAuthorizationHandler, EventAuthorRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler, EventCompetitorRequirementHandler>();

builder.Services.AddScoped<ITokenHandler, TokenHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("HttpClient");

var authenticationOptions = new AuthenticationOptions();
builder.Configuration.GetSection(nameof(AuthenticationOptions)).Bind(authenticationOptions);
builder.Services.AddSingleton(authenticationOptions);
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(cfg =>
{
    cfg.RequireHttpsMetadata = false;
    cfg.SaveToken = true;
    cfg.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = authenticationOptions.JwtIssuer,
        ValidAudience = authenticationOptions.JwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.JwtKey))
    };
});

builder.Services.AddAuthorizationBuilder().AddPolicy("LoggedIn", policy => policy.RequireAuthenticatedUser());

builder.Services.AddDbContext<ApiDBContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("StravaRaceConnectionString"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("StravaRaceConnectionString")));
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
builder.Logging.ClearProviders();
builder.Host.UseNLog();

var app = builder.Build();

app.MapGet("user/{id:int}", async (ApiDBContext db, int id, IAthleteClient athleteClient) =>
    {
        var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
            throw new NotFoundException($"User with id {id} not found");

        var athlete = await athleteClient.GetAthleteAsync();
        return athlete;
    })
    .Produces<AthleteDTO>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError)
    .RequireAuthorization("LoggedIn");

app.MapGet("events", async (IEventService service, IMapper map) =>
    {
        var eventsToReturn = await service.GetAllEvents();
        var eventsDto = map.Map<List<AllEventDTO>>(eventsToReturn);
        return Results.Ok(eventsDto);
    })
    .Produces<List<AllEventDTO>>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError)
    .RequireAuthorization("LoggedIn");

app.MapGet("event/{id}", async (ulong id, IEventService service, IMapper map) =>
    {
        var eventToReturn = await service.GetEvent(id);
        var eventDto = map.Map<ShowEventDTO>(eventToReturn);
        return Results.Ok(eventDto);
    })
    .Produces<ShowEventDTO>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError)
    .RequireAuthorization("LoggedIn");

app.MapGet("user/starred/", async (ApiDBContext db, ISegmentClient segmentClient, IUserContextService userContext) =>
    {
        var id = userContext.GetUserId;
        var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
            throw new NotFoundException($"User with id {id} not found");

        var starred = await segmentClient.GetStarredSegmentsAsync();
        return Results.Ok(starred);
    })
    .Produces<SegmentDTO>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError)
    .RequireAuthorization("LoggedIn");

app.MapPost("event", async (ApiDBContext db, [FromBody] CreateEventDTO dto, IEventService service, IMapper map) =>
    {
        var user = await db.Users.Include(x => x.Token).FirstOrDefaultAsync(x => x.Id == dto.CreatorId);
        if (user is null)
            throw new NotFoundException($"User with id {dto.CreatorId} not found");

        var newEvent = await service.CreateEvent(dto);
        var eventToShow = map.Map<ShowEventDTO>(newEvent);

        return Results.Ok(eventToShow);
    })
    .Produces<ShowEventDTO>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError)
    .RequireAuthorization("LoggedIn");

app.MapPost("event/{id}/addCompetitors", async (ulong id, ApiDBContext db, [FromBody] ModifyEventCompetitorsDTO dto,
        IEventService service, IMapper map, IAuthorizationService authService, IUserContextService userContext) =>
    {
        if (dto.Competitors.Count is 0)
            throw new NotFoundException("No Competitor passed");

        var eventToModify = await db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventToModify is null) throw new NotFoundException($"Event with id {id} not found");

        var authResult =
            await authService.AuthorizeAsync(userContext.User, eventToModify, new EventAuthorRequirement());
        if (!authResult.Succeeded) return Results.Unauthorized();

        await service.AddCompetitors(id, dto.Competitors);

        return Results.Ok();
    })
    .Produces<ShowEventDTO>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status500InternalServerError)
    .RequireAuthorization("LoggedIn");

app.MapPost("event/{id}/addSegments", async (ulong id, ApiDBContext db, [FromBody] ModifyEventSegmentsDTO dto,
        IEventService service, IMapper map, IAuthorizationService authService, IUserContextService userContext) =>
    {
        if (dto.Segments.Count is 0)
            throw new NotFoundException("No Segments passed");

        var eventToModify = await db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (eventToModify is null) throw new NotFoundException($"Event with id {id} not found");

        var authResult =
            await authService.AuthorizeAsync(userContext.User, eventToModify, new EventAuthorRequirement());
        if (!authResult.Succeeded) return Results.Unauthorized();

        await service.AddSegments(id, dto.Segments);

        return Results.Ok();
    })
    .Produces<ShowEventDTO>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status500InternalServerError)
    .RequireAuthorization("LoggedIn");

app.MapPost("event/{id}/addResult", async (ulong id, ApiDBContext db, [FromBody] ResultDTO dto,
        IEventService service, IMapper map, IAuthorizationService authService, IUserContextService userContext) =>
    {
        var eventToModify = await db.Events.Include(e => e.Competitors).FirstOrDefaultAsync(e => e.Id == id);
        if (eventToModify is null) throw new NotFoundException($"Event with id {id} not found");
        
        var authResult =
            await authService.AuthorizeAsync(userContext.User, eventToModify, new EventCompetitorRequirement());
        if (!authResult.Succeeded) return Results.Unauthorized();
        
        await service.AddResult((int)userContext.GetUserId!, id, dto.SegmentId, dto.Time);

        return Results.Ok();
    })
    .Produces<ShowEventDTO>()
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized)
    .Produces(StatusCodes.Status500InternalServerError)
    .RequireAuthorization("LoggedIn");

app.MapPost("connectWithStrava",
        async ([FromBody] object response, IHttpClientFactory clientFactory, IMapper mapper, IUserService service) =>
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
        })
    .Produces(StatusCodes.Status404NotFound);

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();