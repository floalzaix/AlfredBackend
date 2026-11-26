/**
 * Handles global exceptions to avoid
 * returning 500 Internal Server Error
 * and instead return a more user-friendly error message.
 *
 * Expected AppException errors are mapped to their
 * HTTP status. Unexpected exceptions stay as 500.
 */

//
//  Imports
//

namespace Backend.API;

using Backend.Models.Exceptions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

//
//  ErrorHandler
//

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is AppException appException)
        {
            _logger.LogWarning(
                exception,
                "Handled application exception"
            );

            httpContext.Response.StatusCode = appException.StatusCode;

            await httpContext.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = appException.StatusCode,
                    Title = appException.Title,
                    Detail = appException.Message,
                    Instance = httpContext.Request.Path
                },
                cancellationToken
            );

            return true;
        }

        _logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Une erreur inattendue s'est produite.",
                Detail = "Veuillez réessayer plus tard.",
                Instance = httpContext.Request.Path
            },
            cancellationToken
        );

        return true;
    }
}
