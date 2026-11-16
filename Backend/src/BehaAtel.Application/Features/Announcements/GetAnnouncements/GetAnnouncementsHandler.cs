using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Announcements.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Announcements.GetAnnouncements;

public sealed class GetAnnouncementsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAnnouncementsQuery, PagedList<AnnouncementDto>>
{
    public async Task<PagedList<AnnouncementDto>> Handle(
        GetAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        var announcements = dbContext.Announcements.AsNoTracking()
            .Where(announcement => !announcement.IsDeleted);

        if (request.IsPublished.HasValue)
        {
            announcements = request.IsPublished.Value
                ? announcements.Where(announcement => announcement.PublishedAt.HasValue)
                : announcements.Where(announcement => !announcement.PublishedAt.HasValue);
        }

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var search = request.Pagination.Search.Trim().ToLower();
            announcements = announcements.Where(announcement => announcement.Title.ToLower().Contains(search));
        }

        announcements = string.Equals(request.Pagination.SortDir, "asc", StringComparison.OrdinalIgnoreCase)
            ? announcements.OrderBy(announcement => announcement.CreatedAt)
            : announcements.OrderByDescending(announcement => announcement.CreatedAt);

        var query =
            from announcement in announcements
            join author in dbContext.Users.AsNoTracking().Where(user => !user.IsDeleted)
                on announcement.AuthorId equals author.Id
            select new AnnouncementDto(
                announcement.Id,
                announcement.Title,
                announcement.Body,
                announcement.PublishedAt,
                author.FullName,
                announcement.CreatedAt,
                announcement.Audience,
                announcement.BuildingId,
                announcement.ResidentIds);

        return await PagedList<AnnouncementDto>.CreateAsync(
            query,
            request.Pagination.Page,
            request.Pagination.PageSize,
            cancellationToken);
    }
}
