using Microsoft.AspNetCore.Mvc;

namespace BehaAtel.WebApi.Middleware;

public static class ProblemDetailsWriter
{
    public static Task WriteAsync(
        HttpContext context,
        int status,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Type = "about:blank",
            Title = title,
            Status = status,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        return context.Response.WriteAsJsonAsync(
            problem,
            contentType: "application/problem+json",
            options: null,
            cancellationToken: context.RequestAborted);
    }
}
