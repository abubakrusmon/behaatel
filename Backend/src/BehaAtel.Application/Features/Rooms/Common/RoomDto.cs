using BehaAtel.Domain.Enums;

namespace BehaAtel.Application.Features.Rooms.Common;

public sealed record RoomDto(
    Guid Id,
    Guid BuildingId,
    string BuildingName,
    string Number,
    int Floor,
    int Capacity,
    int OccupiedBeds,
    int FreeBeds,
    decimal PricePerBed,
    RoomType Type);
