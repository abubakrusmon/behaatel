namespace BehaAtel.Application.Features.Residents.GetResidentById;

public sealed record OccupancyHistoryDto(
    DateOnly From,
    DateOnly? To,
    string RoomNumber,
    string BuildingName);
