using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Common.Services;
using BehaAtel.Application.Features.Notifications.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Notifications.CreateNotification;

public sealed class CreateNotificationHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditService auditService,
    ITelegramNotifier telegramNotifier)
    : IRequestHandler<CreateNotificationCommand, NotificationSendResultDto>
{
    public async Task<NotificationSendResultDto> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } authorId || authorId == Guid.Empty)
        {
            throw new UnauthorizedException("Не удалось определить автора уведомления.");
        }

        var authorName = await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == authorId && !user.IsDeleted)
            .Select(user => user.FullName)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(User), authorId);

        var recipientsQuery = NotificationRecipientQueries.GetActive(dbContext);
        if (request.AudienceKind == TelegramAudienceKind.Building)
        {
            recipientsQuery = recipientsQuery.Where(recipient => recipient.BuildingId == request.BuildingId);
        }
        else if (request.AudienceKind == TelegramAudienceKind.SelectedResidents)
        {
            var residentIds = request.ResidentIds!.Distinct().ToArray();
            recipientsQuery = recipientsQuery.Where(recipient => residentIds.Contains(recipient.ResidentId));
        }

        var recipients = await recipientsQuery
            .OrderBy(recipient => recipient.FullName)
            .ToListAsync(cancellationToken);
        if (recipients.Count == 0)
        {
            throw new ConflictException("Нет активных жильцов для выбранной аудитории.");
        }

        if (request.AudienceKind == TelegramAudienceKind.SelectedResidents)
        {
            var foundIds = recipients.Select(recipient => recipient.ResidentId).ToHashSet();
            var missingId = request.ResidentIds!.FirstOrDefault(id => !foundIds.Contains(id));
            if (missingId != Guid.Empty)
            {
                throw new NotFoundException(nameof(Resident), missingId);
            }
        }

        var notifications = recipients.Select(recipient =>
            ResidentNotification.Create(recipient.ResidentId, authorId, request.Title, request.Body))
            .ToArray();
        var notificationId = notifications[0].Id;
        dbContext.ResidentNotifications.AddRange(notifications);
        await dbContext.SaveChangesAsync(cancellationToken);

        TelegramQueueStatus? telegramStatus = null;
        if (request.SendTelegram)
        {
            var audience = new TelegramAudience(
                request.AudienceKind,
                recipients.Count,
                request.AudienceKind == TelegramAudienceKind.Building ? recipients[0].BuildingName : null,
                recipients.Select(recipient => recipient.FullName).ToArray());
            var message = TelegramMessageFormatter.Format(
                TelegramMessageKind.Notification,
                request.Title,
                request.Body,
                audience,
                authorName,
                dateTimeProvider.UtcNow);
            var linkedRecipients = recipients
                .Where(recipient => !string.IsNullOrWhiteSpace(recipient.TelegramChatId))
                .Select(recipient => new TelegramRecipientMessage(
                    recipient.TelegramChatId!,
                    message))
                .ToArray();
            telegramStatus = telegramNotifier.Enqueue(new TelegramNotificationJob(
                message,
                linkedRecipients,
                recipients.Count,
                recipients.Count - linkedRecipients.Length,
                IncludeDeliverySummary: true));
        }

        await auditService.LogAsync(
            "CreateNotification",
            nameof(ResidentNotification),
            notificationId.ToString(),
            null,
            new
            {
                request.Title,
                RecipientCount = recipients.Count,
                request.AudienceKind,
                TelegramStatus = telegramStatus
            },
            cancellationToken);

        return new NotificationSendResultDto(notificationId, recipients.Count, telegramStatus);
    }
}
