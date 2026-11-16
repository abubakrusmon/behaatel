using System.Threading.Channels;
using BehaAtel.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BehaAtel.Infrastructure.BackgroundJobs;

public sealed class EmailDeliveryBackgroundService(
    Channel<QueuedEmail> channel,
    SmtpEmailSender sender,
    ILogger<EmailDeliveryBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await sender.SendAsync(email.To, email.Subject, email.HtmlBody, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                // Подробности уже записаны в SmtpEmailSender.
                logger.LogWarning("Письмо не отправлено; пользователь может запросить код повторно.");
            }
        }
    }
}
