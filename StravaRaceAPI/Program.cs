using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using StravaRaceAPI;
using StravaRaceAPI.Api;
using StravaRaceAPI.Api.Clients;
using StravaRaceAPI.Authorization;
using StravaRaceAPI.Endpoints;
using StravaRaceAPI.Entities;
using StravaRaceAPI.Middlewares;
using StravaRaceAPI.Services;
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
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
builder.Logging.ClearProviders();
builder.Host.UseNLog();

var app = builder.Build();

app.MapEventEndpoints();
app.MapUserEndpoints();

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