namespace BehaAtel.Application.Features.Rooms.Common;

public sealed record RoomDetailsDto(
    Guid Id,
    Guid BuildingId,
    string BuildingName,
    string Number,
    int Floor,
    int Capacity,
    int OccupiedBeds,
    int FreeBeds,
    decimal PricePerBed,
    BehaAtel.Domain.Enums.RoomType Type,
    IReadOnlyCollection<RoomOccupantDto> Residents);
