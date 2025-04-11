using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Metrics.Web.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // log the error


        // determine request type (razor page or api)
        bool isApiRequest = httpContext.Request.Path.StartsWithSegments("/api")
            || httpContext.Request.Headers.Accept.Any(
                h => h != null && h.Contains("application/json"));

        if (isApiRequest)
        {
            return await HandleApiExceptionAsync(httpContext, exception, cancellationToken);
        }
        else
        {
            // httpContext.Response.Redirect($"/Error?code={StatusCodes.Status500InternalServerError}&message={Uri.EscapeDataString(exception.Message)}");
            // httpContext.Response.Redirect($"/Error/{StatusCodes.Status500InternalServerError}");
            httpContext.Response.Redirect($"/error/{StatusCodes.Status500InternalServerError}");


            return true;
        }
    }

    private async ValueTask<bool> HandleApiExceptionAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetErrorTitle(exception),
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        // if (_environment.IsDevelopment())
        // {
        //     problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        // }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private string GetErrorTitle(Exception exception)
    {
        return exception switch
        {
            ArgumentException => "Bad Request",
            KeyNotFoundException => "Resource Not Found",
            UnauthorizedAccessException => "Unauthorized",
            _ => "Server Error"
        };
    }
}
