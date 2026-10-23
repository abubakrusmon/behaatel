using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Announcements.DeleteAnnouncement;

public sealed class DeleteAnnouncementHandler(
    IApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    IAuditService auditService)
    : IRequestHandler<DeleteAnnouncementCommand>
{
    public async Task Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await dbContext.Announcements
            .SingleOrDefaultAsync(candidate => candidate.Id == request.Id && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcement), request.Id);

        announcement.MarkDeleted(dateTimeProvider.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("DeleteAnnouncement", nameof(Announcement), announcement.Id.ToString(),
            new { IsDeleted = false }, new { IsDeleted = true, announcement.DeletedAt }, cancellationToken);
    }
}
