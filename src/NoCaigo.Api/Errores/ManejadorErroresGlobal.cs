using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace NoCaigo.Api.Errores;

/// <summary>
/// Captura cualquier excepción no controlada, la registra en el log y responde un
/// ProblemDetails (RFC 9457) genérico: el cliente nunca ve el stack trace ni detalles internos.
/// </summary>
public sealed class ManejadorErroresGlobal(
    IProblemDetailsService problemDetails,
    ILogger<ManejadorErroresGlobal> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // El cliente cerró la conexión: no es un error del servidor, no hay a quién responder.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            return true;

        logger.LogError(exception, "Error no controlado en {Metodo} {Ruta}",
            httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Ocurrió un error inesperado.",
                Detail = "Intenta nuevamente en unos minutos."
            }
        });
    }
}
