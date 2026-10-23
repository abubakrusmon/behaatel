using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Announcements.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Announcements.CreateAnnouncement;

public sealed class CreateAnnouncementHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditService auditService)
    : IRequestHandler<CreateAnnouncementCommand, AnnouncementDto>
{
    public async Task<AnnouncementDto> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } authorId || authorId == Guid.Empty)
        {
            throw new UnauthorizedException("Не удалось определить автора объявления.");
        }

        var author = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == authorId && !user.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), authorId);
        var announcement = Announcement.Create(request.Title, request.Body, authorId);
        if (request.PublishNow)
        {
            announcement.Publish(dateTimeProvider.UtcNow);
        }

        dbContext.Announcements.Add(announcement);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("CreateAnnouncement", nameof(Announcement), announcement.Id.ToString(),
            null, new { announcement.Title, announcement.Body, announcement.PublishedAt }, cancellationToken);

        return new AnnouncementDto(
            announcement.Id,
            announcement.Title,
            announcement.Body,
            announcement.PublishedAt,
            author.FullName,
            announcement.CreatedAt);
    }
}
