using BehaAtel.Application.Common.Interfaces;

namespace BehaAtel.Application.Features.Notifications.CreateNotification;

public sealed record NotificationSendResultDto(
    Guid Id,
    int RecipientCount,
    TelegramQueueStatus? TelegramStatus);
