using StravaRaceAPI.Exceptions;

namespace StravaRaceAPI.Middlewares;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly Logger<ErrorHandlingMiddleware> _logger;
    public ErrorHandlingMiddleware(Logger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (ExceptionWithStatusCode e)
        {
            context.Response.StatusCode = (int)e.StatusCode;
            await context.Response.WriteAsync(e.Message);
            _logger.LogError(e, e.Message);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }
}