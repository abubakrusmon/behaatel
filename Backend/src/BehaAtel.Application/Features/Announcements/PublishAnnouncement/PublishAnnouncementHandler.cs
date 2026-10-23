using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Announcements.PublishAnnouncement;

public sealed class PublishAnnouncementHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IAuditService auditService)
    : IRequestHandler<PublishAnnouncementCommand>
{
    public async Task Handle(PublishAnnouncementCommand request, CancellationToken cancellationToken)
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
        await auditService.LogAsync("PublishAnnouncement", nameof(Announcement), announcement.Id.ToString(),
            new { PublishedAt = (DateTime?)null }, new { announcement.PublishedAt }, cancellationToken);
    }
}
