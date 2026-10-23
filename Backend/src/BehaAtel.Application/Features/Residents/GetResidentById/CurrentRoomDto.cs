namespace BehaAtel.Application.Features.Residents.GetResidentById;

public sealed record CurrentRoomDto(
    string BuildingName,
    string RoomNumber,
    int Floor,
    decimal PricePerBed);
