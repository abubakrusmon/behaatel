using BehaAtel.Application.Common.Exceptions;
using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Features.Announcements.Common;
using BehaAtel.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Announcements.UpdateAnnouncement;

public sealed class UpdateAnnouncementHandler(IApplicationDbContext dbContext, IAuditService auditService)
    : IRequestHandler<UpdateAnnouncementCommand, AnnouncementDto>
{
    public async Task<AnnouncementDto> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await dbContext.Announcements
            .SingleOrDefaultAsync(candidate => candidate.Id == request.Id && !candidate.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Announcement), request.Id);
        var author = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == announcement.AuthorId && !user.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(User), announcement.AuthorId);

        var oldValues = new { announcement.Title, announcement.Body };
        announcement.Update(request.Title, request.Body);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("UpdateAnnouncement", nameof(Announcement), announcement.Id.ToString(),
            oldValues, new { announcement.Title, announcement.Body }, cancellationToken);

        return new AnnouncementDto(
            announcement.Id,
            announcement.Title,
            announcement.Body,
            announcement.PublishedAt,
            author.FullName,
            announcement.CreatedAt);
    }
}
