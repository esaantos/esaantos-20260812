using GestaoColaboradores.API.Services.Common.Exceptions;

namespace GestaoColaboradores.API.Infra.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (TryMapStatusCode(ex, out var statusCode))
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }

    private static bool TryMapStatusCode(Exception ex, out int statusCode)
    {
        statusCode = ex switch
        {
            BadRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            UnprocessableEntityException => StatusCodes.Status422UnprocessableEntity,
            _ => 0
        };

        return statusCode != 0;
    }
}
