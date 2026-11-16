using BehaAtel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BehaAtel.Infrastructure.BackgroundJobs;

public sealed class EmailCodeCleanupBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var expired = await db.EmailCodes
                .Where(code => code.ExpiresAt < DateTime.UtcNow.AddHours(-1))
                .ToListAsync(stoppingToken);
            db.EmailCodes.RemoveRange(expired);
            var expiredTelegramAttempts = await db.TelegramLinkAttempts
                .Where(attempt => attempt.ExpiresAt < DateTime.UtcNow.AddHours(-1))
                .ToListAsync(stoppingToken);
            db.TelegramLinkAttempts.RemoveRange(expiredTelegramAttempts);
            await db.SaveChangesAsync(stoppingToken);
        }
    }
}
