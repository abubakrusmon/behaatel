using BehaAtel.Application.Common.Models;

namespace BehaAtel.Application.Common.Interfaces;

public interface ITelegramNotifier
{
    bool IsConfigured { get; }

    TelegramQueueStatus Enqueue(TelegramNotificationJob job);
}

public enum TelegramQueueStatus
{
    Queued,
    NotConfigured
}
