using System.Threading.Channels;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BehaAtel.Infrastructure.Services;

public sealed record TelegramQueuedMessage(
    string AdminChatId,
    TelegramNotificationJob Job);

public sealed class TelegramMessageQueue(
    Channel<TelegramQueuedMessage> channel,
    IOptions<TelegramSettings> options,
    ILogger<TelegramMessageQueue> logger) : ITelegramNotifier
{
    private static int _configurationWarningLogged;
    private readonly TelegramSettings _settings = options.Value;

    public bool IsConfigured =>
        _settings.Enabled &&
        !string.IsNullOrWhiteSpace(_settings.Token);

    public TelegramQueueStatus Enqueue(TelegramNotificationJob job)
    {
        if (!IsConfigured)
        {
            if (Interlocked.Exchange(ref _configurationWarningLogged, 1) == 0)
            {
                logger.LogWarning("Telegram is not configured; messages will not be queued.");
            }

            return TelegramQueueStatus.NotConfigured;
        }

        if (!channel.Writer.TryWrite(new TelegramQueuedMessage(_settings.ChatId?.Trim() ?? string.Empty, job)))
        {
            logger.LogError("Telegram message could not be added to the delivery queue.");
            return TelegramQueueStatus.NotConfigured;
        }

        return TelegramQueueStatus.Queued;
    }
}
