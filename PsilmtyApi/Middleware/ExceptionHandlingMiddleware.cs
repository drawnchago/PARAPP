using System.Net;

namespace PsilmtyApi.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteProblemAsync(context, HttpStatusCode.Unauthorized, exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API error.");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "An unexpected server error occurred.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, HttpStatusCode status, string detail)
    {
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsJsonAsync(new
        {
            status = (int)status,
            title = status.ToString(),
            detail
        });
    }
}
