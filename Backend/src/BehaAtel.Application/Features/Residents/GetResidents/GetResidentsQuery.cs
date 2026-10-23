using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Residents.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Residents.GetResidents;

public sealed record GetResidentsQuery(
    PaginationQuery Pagination,
    ResidentStatus? Status = null,
    Guid? BuildingId = null,
    Guid? RoomId = null,
    bool? HasDebt = null) : IRequest<PagedList<ResidentListItemDto>>;
