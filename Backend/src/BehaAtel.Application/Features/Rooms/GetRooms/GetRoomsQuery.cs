using BehaAtel.Application.Common.Models;
using BehaAtel.Application.Features.Rooms.Common;
using BehaAtel.Domain.Enums;
using MediatR;

namespace BehaAtel.Application.Features.Rooms.GetRooms;

public sealed record GetRoomsQuery(
    PaginationQuery Pagination,
    Guid? BuildingId = null,
    int? Floor = null,
    RoomType? Type = null,
    bool? OnlyWithFreeBeds = null) : IRequest<PagedList<RoomDto>>;
