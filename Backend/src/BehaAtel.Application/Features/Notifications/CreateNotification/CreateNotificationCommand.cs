using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Notifications.Common;
using MediatR;

namespace BehaAtel.Application.Features.Notifications.CreateNotification;

public sealed record CreateNotificationCommand(
    string Title,
    string Body,
    TelegramAudienceKind AudienceKind,
    Guid? BuildingId,
    IReadOnlyCollection<Guid>? ResidentIds,
    bool SendTelegram = false) : IRequest<NotificationSendResultDto>;
