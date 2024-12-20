using StravaRaceAPI.Exceptions;

namespace StravaRaceAPI.Middlewares;

public class ErrrorHandlingMiddleware : IMiddleware
{
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
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}