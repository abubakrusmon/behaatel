using System.Threading.Channels;
using BehaAtel.Application.Common.Models;
using BehaAtel.Infrastructure.Options;
using BehaAtel.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BehaAtel.Infrastructure.BackgroundJobs;

public sealed class TelegramDeliveryBackgroundService(
    Channel<TelegramQueuedMessage> channel,
    IHttpClientFactory httpClientFactory,
    IOptions<TelegramSettings> options,
    ILoggerFactory loggerFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var client = httpClientFactory.CreateClient("TelegramApi");
        var sender = new TelegramApiClient(
            client,
            options,
            loggerFactory.CreateLogger<TelegramApiClient>());
        await foreach (var message in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                var delivered = 0;
                var recipients = message.Job.Recipients ?? [];
                foreach (var recipient in recipients)
                {
                    if (await sender.SendAsync(recipient.ChatId, recipient.Text, stoppingToken))
                    {
                        delivered++;
                    }

                    // Лимит Telegram — около 30 сообщений в секунду.
                    await Task.Delay(TimeSpan.FromMilliseconds(40), stoppingToken);
                }

                loggerFactory.CreateLogger<TelegramDeliveryBackgroundService>().LogInformation(
                    "Telegram: доставлено {Delivered} из {Linked} (всего получателей {Total}, без Telegram {WithoutTelegram}).",
                    delivered, recipients.Count, message.Job.TotalRecipients, message.Job.WithoutTelegram);

                if (string.IsNullOrWhiteSpace(message.AdminChatId))
                {
                    continue;
                }

                var adminText = message.Job.IncludeDeliverySummary
                    ? $"{message.Job.AdminMessage}\n\n<b>Доставлено:</b> {delivered} из {message.Job.TotalRecipients}, без Telegram {message.Job.WithoutTelegram}"
                    : message.Job.AdminMessage;
                await sender.SendAsync(message.AdminChatId, adminText, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                // Одно неудачное сообщение не должно останавливать всю очередь.
                loggerFactory.CreateLogger<TelegramDeliveryBackgroundService>()
                    .LogError("Telegram delivery job failed ({ErrorType}).", exception.GetType().Name);
            }
        }
    }
}
