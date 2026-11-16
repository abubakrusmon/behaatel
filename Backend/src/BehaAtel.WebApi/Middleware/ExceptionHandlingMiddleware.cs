using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Domain.Exceptions;

namespace BehaAtel.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (status, title, detail, errors) = MapException(exception);
            if (status >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Необработанная ошибка запроса {TraceId}.", context.TraceIdentifier);
            }
            else
            {
                logger.LogWarning(
                    "Запрос завершился с HTTP {StatusCode}: {Message}. TraceId: {TraceId}",
                    status,
                    exception.Message,
                    context.TraceIdentifier);
            }

            if (!context.Response.HasStarted)
            {
                await ProblemDetailsWriter.WriteAsync(context, status, title, detail, errors);
            }
        }
    }

    private static (int Status, string Title, string Detail, IDictionary<string, string[]>? Errors)
        MapException(Exception exception) =>
        exception switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "Ошибка проверки данных.",
                validation.Message,
                validation.Errors),
            UnauthorizedException => (
                StatusCodes.Status401Unauthorized,
                "Не удалось выполнить вход.",
                exception.Message,
                null),
            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                "Доступ запрещён.",
                exception.Message,
                null),
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Ресурс не найден.",
                exception.Message,
                null),
            ConflictException => (
                StatusCodes.Status409Conflict,
                "Конфликт данных.",
                exception.Message,
                null),
            DomainException => (
                StatusCodes.Status400BadRequest,
                "Ошибка доменных правил.",
                exception.Message,
                null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Внутренняя ошибка сервера.",
                "Во время обработки запроса произошла ошибка.",
                null)
        };
}
