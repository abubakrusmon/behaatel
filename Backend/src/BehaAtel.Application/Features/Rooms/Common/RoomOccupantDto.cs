namespace BehaAtel.Application.Features.Rooms.Common;

public sealed record RoomOccupantDto(Guid ResidentId, string FullName, string? Phone, DateOnly From);
