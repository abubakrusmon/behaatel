using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Common.Services;
using BehaAtel.Application.Features.Notifications.Common;
using BehaAtel.Domain.Entities;
using BehaAtel.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Announcements.Common;

/// <summary>Отправляет опубликованное объявление в Telegram жильцам из его аудитории.</summary>
internal static class AnnouncementTelegramDispatcher
{
    public static async Task<TelegramQueueStatus> DispatchAsync(
        IApplicationDbContext dbContext,
        ITelegramNotifier telegramNotifier,
        Announcement announcement,
        string authorName,
        CancellationToken cancellationToken)
    {
        var recipientsQuery = NotificationRecipientQueries.GetActive(dbContext);
        if (announcement.Audience == AnnouncementAudience.Building)
        {
            var buildingId = announcement.BuildingId;
            recipientsQuery = recipientsQuery.Where(recipient => recipient.BuildingId == buildingId);
        }
        else if (announcement.Audience == AnnouncementAudience.SelectedResidents)
        {
            var residentIds = announcement.ResidentIds;
            recipientsQuery = recipientsQuery.Where(recipient => residentIds.Contains(recipient.ResidentId));
        }

        var recipients = await recipientsQuery.ToListAsync(cancellationToken);
        var linked = recipients.Where(recipient => !string.IsNullOrWhiteSpace(recipient.TelegramChatId)).ToArray();

        var audience = new TelegramAudience(
            (TelegramAudienceKind)(int)announcement.Audience,
            recipients.Count,
            announcement.Audience == AnnouncementAudience.Building ? recipients.FirstOrDefault()?.BuildingName : null,
            recipients.Select(recipient => recipient.FullName).Take(10).ToArray());
        var message = TelegramMessageFormatter.Format(
            TelegramMessageKind.Announcement,
            announcement.Title,
            announcement.Body,
            audience,
            authorName,
            announcement.PublishedAt!.Value);

        return telegramNotifier.Enqueue(new TelegramNotificationJob(
            message,
            linked.Select(recipient => new TelegramRecipientMessage(recipient.TelegramChatId!, message)).ToArray(),
            recipients.Count,
            recipients.Count - linked.Length,
            IncludeDeliverySummary: true));
    }
}
