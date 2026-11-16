using BehaAtel.Application.Common.Interfaces;
using BehaAtel.Application.Common.Models;
using BehaAtel.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BehaAtel.Application.Features.Me.GetMyAnnouncements;

public sealed class GetMyAnnouncementsHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    : IRequestHandler<GetMyAnnouncementsQuery, PagedList<MyAnnouncementDto>>
{
    public async Task<PagedList<MyAnnouncementDto>> Handle(
        GetMyAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        var residentId = await dbContext.Residents.AsNoTracking()
            .Where(resident => !resident.IsDeleted && resident.UserId == userId)
            .Select(resident => (Guid?)resident.Id)
            .SingleOrDefaultAsync(cancellationToken);
        var buildingId = residentId is null
            ? null
            : await (
                from occupancy in dbContext.Occupancies.AsNoTracking()
                where occupancy.ResidentId == residentId && occupancy.To == null
                join room in dbContext.Rooms.AsNoTracking() on occupancy.RoomId equals room.Id
                select (Guid?)room.BuildingId)
                .FirstOrDefaultAsync(cancellationToken);
        var currentResidentId = residentId ?? Guid.Empty;

        var query = dbContext.Announcements.AsNoTracking()
            .Where(announcement => !announcement.IsDeleted && announcement.PublishedAt.HasValue)
            .Where(announcement =>
                announcement.Audience == AnnouncementAudience.AllResidents ||
                (announcement.Audience == AnnouncementAudience.Building &&
                 buildingId != null && announcement.BuildingId == buildingId) ||
                (announcement.Audience == AnnouncementAudience.SelectedResidents &&
                 announcement.ResidentIds.Contains(currentResidentId)))
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
