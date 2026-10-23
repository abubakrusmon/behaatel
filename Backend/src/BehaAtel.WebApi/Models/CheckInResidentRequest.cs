namespace BehaAtel.WebApi.Models;

public sealed record CheckInResidentRequest(Guid RoomId, DateOnly Date);
