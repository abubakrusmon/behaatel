namespace BehaAtel.Application.Features.Buildings.Common;

public sealed record BuildingDto(
    Guid Id,
    string Name,
    string Address,
    int FloorsCount,
    int TotalRooms,
    int TotalCapacity,
    int OccupiedBeds);
