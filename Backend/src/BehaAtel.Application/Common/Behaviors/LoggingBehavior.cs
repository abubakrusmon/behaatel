using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BehaAtel.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            return await next();
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation("Запрос {RequestName} выполнен за {ElapsedMilliseconds} мс",
                typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
        }
    }
}
