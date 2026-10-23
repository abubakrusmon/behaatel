using BehaAtel.Application.Common.Models;
using MediatR;

namespace BehaAtel.Application.Features.Me.GetMyAnnouncements;

public sealed record GetMyAnnouncementsQuery(PaginationQuery Pagination)
    : IRequest<PagedList<MyAnnouncementDto>>;
