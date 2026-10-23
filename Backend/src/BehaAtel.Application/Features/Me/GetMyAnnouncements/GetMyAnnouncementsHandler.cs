using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Me.GetMyAnnouncements;

public sealed class GetMyAnnouncementsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyAnnouncementsQuery, PagedList<MyAnnouncementDto>>
{
    public async Task<PagedList<MyAnnouncementDto>> Handle(
        GetMyAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Announcements.AsNoTracking()
            .Where(announcement => !announcement.IsDeleted && announcement.PublishedAt.HasValue)
            .OrderByDescending(announcement => announcement.PublishedAt)
            .Select(announcement => new MyAnnouncementDto(
                announcement.Id,
                announcement.Title,
                announcement.Body,
                announcement.PublishedAt!.Value));

        return await PagedList<MyAnnouncementDto>.CreateAsync(
            query,
            request.Pagination.Page,
            request.Pagination.PageSize,
            cancellationToken);
    }
}
