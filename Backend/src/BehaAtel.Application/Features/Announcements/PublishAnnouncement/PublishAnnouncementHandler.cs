using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Announcements.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Announcements.PublishAnnouncement;

public sealed class PublishAnnouncementHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IAuditService auditService,
    ITelegramNotifier telegramNotifier)
    : IRequestHandler<PublishAnnouncementCommand, TelegramQueueStatus?>
{
    public async Task<TelegramQueueStatus?> Handle(
        PublishAnnouncementCommand request,
        CancellationToken cancellationToken)
    {
        var announcement = await dbContext.Announcements
            .SingleOrDefaultAsync(candidate => candidate.Id == request.Id && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcement), request.Id);

        if (announcement.PublishedAt.HasValue)
        {
            throw new ConflictException("Объявление уже опубликовано.");
        }
        announcement.Publish(dateTimeProvider.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        TelegramQueueStatus? telegramStatus = null;
        if (request.SendTelegram)
        {
            var authorName = await dbContext.Users.AsNoTracking()
                .Where(user => user.Id == announcement.AuthorId)
                .Select(user => user.FullName)
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(nameof(User), announcement.AuthorId);
            telegramStatus = await AnnouncementTelegramDispatcher.DispatchAsync(
                dbContext, telegramNotifier, announcement, authorName, cancellationToken);
        }

        await auditService.LogAsync("PublishAnnouncement", nameof(Announcement), announcement.Id.ToString(),
            new { PublishedAt = (DateTime?)null }, new { announcement.PublishedAt }, cancellationToken);
        return telegramStatus;
    }
}
