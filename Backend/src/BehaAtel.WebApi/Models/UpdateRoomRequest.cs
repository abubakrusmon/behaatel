using BehaAtel.Domain.Enums;

namespace BehaAtel.WebApi.Models;

public sealed record UpdateRoomRequest(
    string Number,
    int Floor,
    int Capacity,
    decimal PricePerBed,
    RoomType Type);
