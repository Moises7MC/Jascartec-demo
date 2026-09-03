using Jascartec.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Jascartec.Api.Middleware;

/// <summary>
/// Traduce las excepciones de Application a respuestas HTTP consistentes (ProblemDetails),
/// para que los controllers no necesiten repetir try/catch en cada acción.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "No autorizado"),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Error no controlado procesando {Path}", context.Request.Path);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
