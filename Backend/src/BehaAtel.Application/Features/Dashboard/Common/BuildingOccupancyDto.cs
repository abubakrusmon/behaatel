namespace BehaAtel.Application.Features.Dashboard.Common;

public sealed record BuildingOccupancyDto(
    Guid BuildingId,
    string BuildingName,
    int TotalBeds,
    int OccupiedBeds,
    decimal OccupancyPercent);
