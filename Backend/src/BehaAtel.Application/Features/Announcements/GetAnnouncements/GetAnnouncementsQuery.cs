using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Announcements.Common;
using MediatR;

namespace BehaAtel.Application.Features.Announcements.GetAnnouncements;

public sealed record GetAnnouncementsQuery(
    PaginationQuery Pagination,
    bool? IsPublished = null) : IRequest<PagedList<AnnouncementDto>>;
