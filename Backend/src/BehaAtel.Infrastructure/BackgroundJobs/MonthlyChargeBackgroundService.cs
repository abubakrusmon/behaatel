using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Charges.GenerateMonthlyCharges;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BehaAtel.Infrastructure.BackgroundJobs;

public sealed class MonthlyChargeBackgroundService(
    IServiceScopeFactory scopeFactory,
    IDateTimeProvider dateTimeProvider,
    ILogger<MonthlyChargeBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DateOnly? generatedPeriod = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = dateTimeProvider.UtcNow;
            var period = new DateOnly(now.Year, now.Month, 1);
            if (now.Day == 1 && generatedPeriod != period)
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(new GenerateMonthlyChargesCommand(period), stoppingToken);
                    generatedPeriod = period;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Не удалось сгенерировать начисления за {Period}. Повтор через 5 минут.",
                        period);
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }
            }

            var nextMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            var delay = nextMonth - dateTimeProvider.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
