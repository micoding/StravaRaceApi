// Global using directives

global using System.Net;
global using System.Security.Claims;
global using System.Text;
global using System.Text.Json.Serialization;
global using AutoMapper;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Models;
global using StravaRaceAPI.Api.Clients;
global using StravaRaceAPI.Authorization;
global using StravaRaceAPI.Entities;
global using StravaRaceAPI.Exceptions;
global using StravaRaceAPI.Models;
global using StravaRaceAPI.Services;
global using AuthenticationOptions = StravaRaceAPI.AuthenticationOptions;
global using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
global using TokenHandler = StravaRaceAPI.Api.TokenHandler;
global using NLog;
global using NLog.Web;
global using StravaRaceAPI;
global using StravaRaceAPI.Api;
global using StravaRaceAPI.Endpoints;
global using StravaRaceAPI.Middlewares;